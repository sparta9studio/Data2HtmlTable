import './style.scss';
import './highlightjs-line-number.css';
import 'highlight.js/styles/vs.css';

import queryString from 'query-string';
import axios from 'axios';
import hljs from 'highlight.js/lib/highlight';
import sql from 'highlight.js/lib/languages/sql';
import 'highlightjs-line-numbers.js'
import * as dom from './dom';

hljs.registerLanguage('sql', sql);

// get base url
const uri = queryString.parseUrl(window.location.toString());
const baseUrl = (uri.url.match(/^(.+)\/$/) || [, uri.url])[1];
const tableNameRegex = (/(^[A-z]\w*$)/);
let __editor = dom.id('editor'),
    __codeBlock = dom.id('code-block');

function previewTable(tableName) {
    processResult(fetch('/getTable',{ tableName }));
}

function doQuery(){
    const query = __editor.value;
    if(!query || !query.trim()){
        return;
    }
    processResult(fetch('/doQuery',{ query }));
}

function fetch(path, params){
    return axios({
        method: 'get',
        url: (baseUrl + path + '?' + queryString.stringify(params)),
        headers: {
            "Accept": 'application/json',
            "Content-Type": 'application/json;charset=utf-8',
            "withCredentials": true
        }
    })
}

function processResult(asyncRequest){
    asyncRequest
        .then(response => response.data)
        .then(payload => {
            renderTable(payload.Columns, payload.Data);
            setQuery(payload.Sql);
            showEditor(false);

            __editor.value = payload.Sql;
        });
}

function renderTable(columns, data) {
    let header = dom.createElement('tr');
    dom.appendTo(header, columns.map(c => {
        let th = dom.createElement('th');
        dom.appendTo(th, dom.createTextNode(c.ColumnName));
        return th;
    }));

    const body = data.map(r => {
        let cells = columns.map(c => {
            let td = dom.createElement('td');
            dom.appendTo(td, createNullableNode(r[c.ColumnName]));
            return td;
        });

        let tr = dom.createElement('tr');
        dom.appendTo(tr, cells);
        return tr;
    });

    const $table = dom.id('table');
    $table.innerHTML = '';
    dom.appendTo($table, header);
    dom.appendTo($table, body);
}

function createNullableNode(value) {
    let node = dom.createTextNode(value);
    if (value === null) {
        node.className = 'is-null';
    }

    return node;
}

function setQuery(sql) {
    const code = dom.id('code');
    code.innerHTML = '';
    code.appendChild(document.createTextNode(sql));
    hljs.highlightBlock(code);
    window.hljs.lineNumbersBlock(code);
}

function attachEvents() {
    __editor.addEventListener('blur', _ => {
        setQuery(__editor.value);
        showEditor(false);
    });

    __codeBlock.addEventListener('click', e => {
        e.preventDefault();
        __editor.style.height = __codeBlock.offsetHeight + 'px';

        showEditor(true);
    });

    dom.id('btn-run').addEventListener('click', doQuery);
}

function showEditor(isShow) {
    const statement = dom.id('statement');
    const isEditing = 'is-editing';
    const hasClassEditing = statement.className.indexOf(isEditing) >= 0;

    if (isShow) {
        if (!hasClassEditing) {
            statement.className += ' ' + isEditing;
        }
        __editor.focus();
    } else if (!isShow && hasClassEditing) {
        statement.className = statement.className.replace(isEditing, '').trim();
    }
}

// start
attachEvents();

let tableName = uri.query.name;
if (tableName) {
    if (tableName.match(tableNameRegex)) {
        previewTable(tableName)
    } else {
        alert(`Table name "${tableName}" is not valid`);
    }
}
