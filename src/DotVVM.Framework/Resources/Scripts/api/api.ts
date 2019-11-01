import { dotvvm } from '../dotvvm-root';
import eventHub from './eventHub';
import { deserialize } from '../serialization/deserialize';

type ApiComputed<T> = 
    KnockoutObservable<T | null> & { 
        refreshValue: (throwOnError?: boolean) => PromiseLike<any> | undefined 
    };

type Result<T> = 
    { type: 'error', error: any } | 
    { type: 'result', result: T };

let cachedValues: { 
    [key: string]: KnockoutObservable<any> 
} = {};

export function invoke<T>(
    callback: () => PromiseLike<T>, 
    refreshTriggers: (KnockoutObservable<any> | string)[] = [], 
    notifyTriggers: string[] = [], 
    commandId = callback.toString()
) : ApiComputed<T> {

    let cachedValue = cachedValues[commandId] || (cachedValues[commandId] = ko.observable<any>(null));

    const load : () => Result<PromiseLike<any>> = () => {
        try {
            var result : PromiseLike<any> = window["Promise"].resolve(ko.ignoreDependencies(callback));
            return { type: 'result', result: result.then((val) => {
                if (val) {
                    cachedValue(ko.unwrap(deserialize(val, cachedValue)));
                    cachedValue.notifySubscribers();
                }
                for (var t of notifyTriggers)
                    eventHub.notify(t);
                return val;
            }, console.warn) };
        }
        catch (e) {
            console.warn(e);
            return { type: 'error', error: e };
        }
    };

    const cmp = <ApiComputed<T>><any>ko.pureComputed(() => cachedValue());

    cmp.refreshValue = (throwOnError) => {
        let promise: Result<PromiseLike<any>> = <any>cachedValue["promise"];
        if (!cachedValue["isLoading"])
        {
            cachedValue["isLoading"] = true;
            promise = load();
            cachedValue["promise"] = promise;
        }
        if (promise.type == 'error')
        {
            cachedValue["isLoading"] = false;
            if (throwOnError) throw promise.error;
            else return;
        }
        else
        {
            promise.result.then(p => cachedValue["isLoading"] = false, p => cachedValue["isLoading"] = false);
            return promise.result;
        }
    };
    if (!cachedValue.peek()) cmp.refreshValue();
    ko.computed(() => refreshTriggers.map(f => typeof f == "string" ? eventHub.get(f)() : f())).subscribe(p => cmp.refreshValue());
    return cmp;
}

export function refreshOn<T>(
    value: ApiComputed<T>, 
    refreshOn: KnockoutObservable<any>
) : ApiComputed<T> {
    if (typeof value.refreshValue != "function") console.error(`The object is not refreshable.`);
    refreshOn.subscribe(() => {
        if (typeof value.refreshValue != "function") console.error(`The object is not refreshable.`);
        value.refreshValue && value.refreshValue();
    });
    return value;
}

dotvvm.api = export;
