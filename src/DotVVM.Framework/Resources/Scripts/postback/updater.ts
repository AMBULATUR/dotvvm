import { getElementByDotvvmId } from '../utils/dom'
import { getViewModel, replaceViewModel } from '../dotvvm-base'
import { deserialize } from '../serialization/deserialize'

var isViewModelUpdating: boolean = false;

export function cleanUpdatedControls(resultObject: any, updatedControls: any = {}) {
    for (const id of Object.keys(resultObject.updatedControls)) {
        var control = getElementByDotvvmId(id);
        if (control) {
            var dataContext = ko.contextFor(control);
            var nextSibling = control.nextSibling;
            var parent = control.parentNode;
            ko.removeNode(control);
            updatedControls[id] = { control: control, nextSibling: nextSibling, parent: parent, dataContext: dataContext };
        }
    }
    return updatedControls;
}

export function restoreUpdatedControls(resultObject: any, updatedControls: any, applyBindingsOnEachControl: boolean) {
    for (const id of Object.keys(resultObject.updatedControls)) {
        var updatedControl = updatedControls[id];
        if (updatedControl) {
            var wrapper = document.createElement(updatedControls[id].parent.tagName || "div");
            wrapper.innerHTML = resultObject.updatedControls[id];
            if (wrapper.childElementCount > 1) throw new Error("Postback.Update control can not render more than one element");
            var element = wrapper.firstElementChild;
            if (element.id == null) throw new Error("Postback.Update control always has to render id attribute.");
            if (element.id !== updatedControls[id].control.id) console.log(`Postback.Update control changed id from '${updatedControls[id].control.id}' to '${element.id}'`);
            wrapper.removeChild(element);
            if (updatedControl.nextSibling) {
                updatedControl.parent.insertBefore(element, updatedControl.nextSibling);
            } else {
                updatedControl.parent.appendChild(element);
            }
        }
    }

    if (applyBindingsOnEachControl) {
        window.setTimeout(() => {
            try {
                for (const id of Object.keys(resultObject.updatedControls)) {
                    var updatedControl = getElementByDotvvmId(id);
                    if (updatedControl) {
                        ko.applyBindings(updatedControls[id].dataContext, updatedControl);
                    }
                }
            }
            finally {
            }
        }, 0);
    }
}

export function updateViewModelAndControls(resultObject: any, replaceViewModel: boolean) {
    try 
    {
        isViewModelUpdating = true;
        
        // remove updated controls
        var updatedControls = cleanUpdatedControls(resultObject);

        // update viewmodel
        if (replaceViewModel) {
            const vm = {};
            deserialize(resultObject.viewModel, vm);
            replaceViewModel(vm);
        }
        else {
            ko.delaySync.pause();
            deserialize(resultObject.viewModel, getViewModel());
            ko.delaySync.resume();
        }

        // remove updated controls which were previously removed from DOM
        cleanUpdatedControls(resultObject, updatedControls);

        // add new updated controls
        restoreUpdatedControls(resultObject, updatedControls, true);
    }
    finally {
        isViewModelUpdating = false;
    }
}

export function patchViewModel(source: any, patch: any): any {
    if (source instanceof Array && patch instanceof Array) {
        return patch.map((val, i) => patchViewModel(source[i], val));
    }
    else if (source instanceof Array || patch instanceof Array)
        return patch;
    else if (typeof source == "object" && typeof patch == "object" && source && patch) {
        for (const p of Object.keys(patch)) {
            if (patch[p] == null) source[p] = null;
            else if (source[p] == null) source[p] = patch[p];
            else source[p] = patchViewModel(source[p], patch[p]);
        }
        return source;
    }
    else return patch;
}
