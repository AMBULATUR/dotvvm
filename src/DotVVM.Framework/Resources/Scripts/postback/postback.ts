import * as counter from './counter'
import { postbackCore } from './postbackCore'
import { getViewModel } from '../dotvvm-base'
import * as internalHandlers from './internal-handlers';
import { DotvvmPostbackError } from '../shared-classes';
import { events } from '../DotVVM.Events';

const globalPostbackHandlers: (ClientFriendlyPostbackHandlerConfiguration)[] = [
    internalHandlers.suppressOnDisabledElementHandler,
    internalHandlers.isPostBackRunningHandler,
    internalHandlers.postbackHandlersStartedEventHandler
];
const globalLaterPostbackHandlers: (ClientFriendlyPostbackHandlerConfiguration)[] = [
    internalHandlers.postbackHandlersCompletedEventHandler,
    internalHandlers.beforePostbackEventPostbackHandler
];

export async function postBack(
        viewModelName: string,
        sender: HTMLElement,
        path: string[],
        command: string,
        controlUniqueId: string,
        context?: any,
        handlers?: ClientFriendlyPostbackHandlerConfiguration[],
        commandArgs?: any[]
    ): Promise<DotvvmAfterPostBackEventArgs> {

    context = context || ko.contextFor(sender);

    const preparedHandlers = findPostbackHandlers(context, globalPostbackHandlers.concat(handlers || []).concat(globalLaterPostbackHandlers));
    if (preparedHandlers.filter(h => h.name && h.name.indexOf("concurrency-") == 0).length == 0) {
        // add a default concurrency handler if none is specified
        preparedHandlers.push(internalHandlers.defaultConcurrencyPostbackHandler);
    }

    const options: PostbackOptions = {
        postbackId: counter.backUpPostBackCounter(),
        sender: sender,
        args: commandArgs || [],
        viewModel: context.$data,
        additionalPostbackData: {}
    };

    const postbackCommit = () => postbackCore(options, path, command, controlUniqueId, context, commandArgs);

    try {
        let wrappedPostbackCommit = await applyPostbackHandlersCore(postbackCommit, options, preparedHandlers);
        var result = await wrappedPostbackCommit();
        events.afterPostback.trigger(result);
    }
    catch (err) {

        if (err instanceof DotvvmPostbackError) {
            const wasInterrupted = err.reason.type == "handler" || err.reason.type == "event";
            const afterPostBackArgsCanceled: DotvvmAfterPostBackEventArgs = {
                serverResponseObject: err.reason.type == "commit" && err.reason.args ? err.reason.args.serverResponseObject : null,
                isHandled: false,
                wasInterrupted,
                commandResult: null,
                viewModel: getViewModel(),
                postbackOptions: options,
                postbackClientId: options.postbackId
            }
            if (wasInterrupted) {
                // trigger afterPostback event
                events.postbackRejected.trigger({})
            } else if (err.reason.type == "network") {
                events.error.trigger({ err:  err.reason.err });
            }
            events.afterPostback.trigger(afterPostBackArgsCanceled);
        }

        throw err;
    }

    return result;
}

function findPostbackHandlers(knockoutContext: KnockoutBindingContext, config: ClientFriendlyPostbackHandlerConfiguration[]) {
    const createHandler = (name: string, options: any) => options.enabled === false ? null : internalHandlers.getPostbackHandler(name)(options);
        return <DotvvmPostbackHandler[]>config.map(h =>
            typeof h == 'string' ? createHandler(h, {}) :
                isPostbackHandler(h) ? h :
                    h instanceof Array ? (() => {
                        const [name, opt] = h;
                        return createHandler(name, typeof opt == "function" ? opt(knockoutContext, knockoutContext.$data) : opt);
                    })() :
                        createHandler(h.name, h.options && h.options(knockoutContext)))
            .filter(h => h != null);
}

type MaybePromise<T> = Promise<T> | T

async function applyPostbackHandlers(next: (options: PostbackOptions) => MaybePromise<PostbackCommitFunction | any>, sender: HTMLElement, handlerConfigurations?: ClientFriendlyPostbackHandlerConfiguration[], args: any[] = [], context = ko.contextFor(sender), viewModel = context.$root, viewModelName?: string): Promise<DotvvmAfterPostBackEventArgs> {
    const saneNext = (options: PostbackOptions) => {
        return wrapCommitFunction(next(options), options)
    }

    const options: PostbackOptions = {
        postbackId: counter.backUpPostBackCounter(),
        sender: sender,
        args: [],
        viewModel: context.$data,
        additionalPostbackData: {}
    };

    const handlers = findPostbackHandlers(context, globalPostbackHandlers.concat(handlerConfigurations || []).concat(globalLaterPostbackHandlers));

    try {
        const commit = await applyPostbackHandlersCore(saneNext, options, handlers);
        const result = await commit()
        return result;
    }
    catch (reason) {
        if (reason) {
            console.log("Rejected: " + reason);
        }
        throw reason
    }
}

async function applyPostbackHandlersCore(next: (options: PostbackOptions) => Promise<PostbackCommitFunction>, options: PostbackOptions, handlers: DotvvmPostbackHandler[]) {

    let fired = false
    next = (options: PostbackOptions) => {
        if (fired) throw new Error("The same postback can't run twice.");
        fired = true
        return next(options)
    }

    const sortedHandlers = sortHandlers(handlers)

    function recursiveCore(index:number): Promise<PostbackCommitFunction> {
        if (index == sortedHandlers.length)
            return next(options)
        else
            return sortedHandlers[index].execute(
                () => recursiveCore(index + 1),
                options
            )
    }
    return recursiveCore(0)
}

function wrapCommitFunction(value: MaybePromise<PostbackCommitFunction | any>, options: PostbackOptions) : Promise<PostbackCommitFunction> {

    return Promise.resolve(value)
           .then(v => {
        if (typeof v == "function") {
            return <PostbackCommitFunction>value;
        }
        else {
            return () => Promise.resolve<DotvvmAfterPostBackEventArgs>({
                postbackOptions: options,
                postbackClientId: options.postbackId,
                serverResponseObject: null,
                commandResult: value,
                wasInterrupted: false,
                isHandled: true,
                viewModel: options.viewModel!
            });
        }
    });
}

export function isPostbackHandler(obj: any): obj is DotvvmPostbackHandler {
    return obj && typeof obj.execute == "function";
}

export function sortHandlers(handlers: DotvvmPostbackHandler[]): DotvvmPostbackHandler[] {
    const getHandler = (() => {
        const handlerMap: { [name: string]: DotvvmPostbackHandler } = {};
        for (const h of handlers) if (h.name != null) {
            handlerMap[h.name] = h;
        }
        return (s: string | DotvvmPostbackHandler) => typeof s == "string" ? handlerMap[s] : s;
    })();
    const dependencies = handlers.map((handler, i) => (handler["@sort_index"] = i, ({ handler, deps: (handler.after || []).map(getHandler) })));
    for (const h of handlers) {
        if (h.before) for (const before of h.before.map(getHandler)) if (before) {
            const index = before["@sort_index"] as number;
            dependencies[index].deps.push(h);
        }
    }

    const result: DotvvmPostbackHandler[] = [];
    const doneBitmap = new Uint8Array(dependencies.length);
    const addToResult = (index: number) => {
        switch (doneBitmap[index]) {
            case 0: break;
            case 1: throw new Error("Cyclic PostbackHandler dependency found.");
            case 2: return; // it's already in the list
            default: throw new Error("");
        }
        if (doneBitmap[index] == 1) return;
        doneBitmap[index] = 1;

        const { handler, deps } = dependencies[index];
        for (const d of deps) {
            addToResult(d["@sort_index"]);
        }

        doneBitmap[index] = 2;
        result.push(handler);
    }
    for (let i = 0; i < dependencies.length; i++) {
        addToResult(i);
    }
    return result;
}

