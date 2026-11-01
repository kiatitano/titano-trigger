'use strict';
if (!window.titano && window.chrome?.webview) {
  let sequence = 0;
  const pending = new Map();
  const subscribers = new Set();
  window.chrome.webview.addEventListener('message', (event) => {
    const message = event.data;
    if (message?.type === 'state') {
      subscribers.forEach((callback) => callback(message.value));
      return;
    }
    if (message?.type === 'response') {
      const item = pending.get(message.id);
      if (!item) return;
      pending.delete(message.id);
      if (message.error) item.reject(new Error(message.error));
      else item.resolve(message.value);
    }
  });
  const call = (op, value) => new Promise((resolve, reject) => {
    const id = ++sequence;
    pending.set(id, { resolve, reject });
    window.chrome.webview.postMessage({ id, op, value });
  });
  window.titano = {
    state: () => call('state'),
    connect: (token) => call('connect', token),
    disconnect: () => call('disconnect'),
    rules: (rules) => call('rules', rules),
    start: () => call('start'),
    stop: () => call('stop'),
    test: (id) => call('test', id),
    link: (service) => call('link', service),
    onState: (callback) => { subscribers.add(callback); return () => subscribers.delete(callback); }
  };
}
