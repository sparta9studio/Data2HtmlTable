function isElement(o) {
    return (
        typeof HTMLElement === "object" ? o instanceof HTMLElement : //DOM2
            o && typeof o === "object" && o !== null && o.nodeType === 1 && typeof o.nodeName === "string"
    );
}

export function id(id) {
    return document.getElementById(id);
}

export function createElement(tag) {
    return document.createElement(tag);
}

export function appendTo(parent, ...nodes) {
    nodes.forEach(n => {
        if (Array.isArray(n)) {
            appendTo.apply(null, [parent, ...n]);
        } else {
            parent.appendChild(n);
        }
    });
}

export function createTextNode(text) {
    let node = createElement('span');
    node.appendChild(document.createTextNode(text));
    return node;
}