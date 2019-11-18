import * as events from '../DotVVM.Events';
import { createPostbackArgs } from "../createPostbackArgs";
import { DotvvmPostbackError } from "../shared-classes";
import { isElementDisabled } from "../utils/dom";
import { getPostbackQueue, enterActivePostback, leaveActivePostback, runNextInQueue } from "./queue";
import { getLastStartedPostbackId } from "./postbackCore";
import { getIsViewModelUpdating } from "./updater";

let postbackCount = 0;
let postbackHandlers: DotvvmPostbackHandlerCollection = {};

export const isPostbackRunning = ko.observable(false)

export const suppressOnDisabledElementHandler: DotvvmPostbackHandler = {
    name: "suppressOnDisabledElement",
    before: ["setIsPostbackRunning", "concurrency-default", "concurrency-queue", "concurrency-deny"],
    execute(next: () => Promise<PostbackCommitFunction>, options: PostbackOptions) {
        if (isElementDisabled(options.sender)) {
            return Promise.reject(new DotvvmPostbackError({
                type: "handler",
                handler: suppressOnDisabledElementHandler,
                message: "PostBack is prohibited on disabled element"
            }))
        }
        else return next()
    }
}

export const beforePostbackEventPostbackHandler: DotvvmPostbackHandler = {
    execute: (next: () => Promise<PostbackCommitFunction>, options: PostbackOptions) => {

        // trigger beforePostback event
        const beforePostbackArgs: DotvvmBeforePostBackEventArgs = {
            ...createPostbackArgs(options),
            cancel: false
        };
        events.beforePostback.trigger(beforePostbackArgs);
        if (beforePostbackArgs.cancel) {
            return Promise.reject(new DotvvmPostbackError({ type: "event", options }));
        }
        return next();
    }
}

export const isPostBackRunningHandler: DotvvmPostbackHandler = {
    name: "setIsPostbackRunning",
    before: ["eventInvoke-postbackHandlersStarted"],
    async execute(next: () => Promise<PostbackCommitFunction>) {
        isPostbackRunning(true)
        postbackCount++
        try {
            return await next()
        }
        finally {
            isPostbackRunning(!!--postbackCount)
        }
    }
}

export const defaultConcurrencyPostbackHandler: DotvvmPostbackHandler = {
    name: "concurrency-default",
    before: ["setIsPostbackRunning"],
    execute: (next: () => Promise<PostbackCommitFunction>, options: PostbackOptions) => {
        return commonConcurrencyHandler(next(), options, o.q || "default")
    }
};

export const denyConcurrencyPostbackHandler: DotvvmPostbackHandler = {
    name: "concurrency-deny",
    before: ["setIsPostbackRunning"],
    execute(next: () => Promise<PostbackCommitFunction>, options: PostbackOptions) {
        var queue = o.q || "default";
        if (getPostbackQueue(queue).noRunning > 0)
            return Promise.reject({ type: "handler", handler: this, message: "An postback is already running" });
        return commonConcurrencyHandler(next(), options, queue);
    }
}

export const queueConcurrencyPostbackHandler: DotvvmPostbackHandler = {
    name: "concurrency-queue",
    before: ["setIsPostbackRunning"],
    execute(next: () => Promise<PostbackCommitFunction>, options: PostbackOptions) {
        var queue = o.q || "default";
        var handler = () => commonConcurrencyHandler(next(), options, queue);

        if (getPostbackQueue(queue).noRunning > 0) {
            return new Promise<PostbackCommitFunction>(resolve => {
                getPostbackQueue(queue).queue.push(() => resolve(handler()));
            })
        }
        return handler();
    }
}

export const suppressOnUpdating: DotvvmPostbackHandler = {
    name: "suppressOnUpdating",
    before: ["setIsPostbackRunning", "concurrency-default", "concurrency-queue", "concurrency-deny"],
    execute(next: () => Promise<PostbackCommitFunction>, options: PostbackOptions) {
        if (getIsViewModelUpdating()) return Promise.reject({ type: "handler", handler: this, message: "ViewModel is updating, so it's probably false onchange event" });
        else return next()
    }
}

function commonConcurrencyHandler<T>(promise: Promise<PostbackCommitFunction>, options: PostbackOptions, queueName: string): Promise<PostbackCommitFunction> {
    enterActivePostback(queueName);

    const dispatchNext = (args: DotvvmAfterPostBackEventArgs) => {
        const drop = () => {
            leaveActivePostback(queueName);
            runNextInQueue(queueName);
        }
        if (args.redirectPromise) {
            args.redirectPromise.then(drop, drop);
        } else {
            drop();
        }
    }

    return promise.then(result => {
        var p = getLastStartedPostbackId() == options.postbackId ?
            result :
            () => Promise.reject(null);
        return () => {
            const pr = p()
            pr.then(dispatchNext, dispatchNext)
            return pr
        };
    }, error => {
        dispatchNext(error)
        return Promise.reject(error)
    });
}

export const postbackHandlersStartedEventHandler: DotvvmPostbackHandler = {
    name: "eventInvoke-postbackHandlersStarted",
    execute: <T>(callback: () => Promise<T>, options: PostbackOptions) => {
        events.postbackHandlersStarted.trigger(options);
        return callback()
    }
}

export const postbackHandlersCompletedEventHandler: DotvvmPostbackHandler = {
    name: "eventInvoke-postbackHandlersCompleted",
    after: ["eventInvoke-postbackHandlersStarted"],
    execute: <T>(callback: () => Promise<T>, options: PostbackOptions) => {
        events.postbackHandlersCompleted.trigger(options);
        return callback()
    }
}

export function getPostbackHandler(name: string) {
    const handler = postbackHandlers[name];
    if (handler) {
        return handler;
    } else {
        throw new Error(`Could not find postback handler of name '${name}'`);
    }
}
