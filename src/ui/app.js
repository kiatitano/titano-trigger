'use strict';
const $ = (id) => document.getElementById(id);
const number = (value) => new Intl.NumberFormat('fa-IR').format(value);
const escapeHtml = (value) => String(value).replace(/[&<>"']/g, (char) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[char]);
let state = { connected: false, armed: false, rules: [], history: [] };
let toastTimer;
const keys = [...'ABCDEFGHIJKLMNOPQRSTUVWXYZ', ...'0123456789', ...Array.from({ length: 12 }, (_, i) => `F${i + 1}`), 'SPACE', 'ENTER', 'TAB', 'ESC', 'UP', 'DOWN', 'LEFT', 'RIGHT', 'MOUSE_LEFT', 'MOUSE_RIGHT', 'MOUSE_MIDDLE'];
$('keySelect').innerHTML = keys.map((key) => `<option value="${key}">${key.replace('MOUSE_', 'ماوس ')}</option>`).join('');

function toast(message, error = false) {
  const element = $('toast');
  element.textContent = message;
  element.className = `toast show${error ? ' error' : ''}`;
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => { element.className = 'toast'; }, 4500);
}
async function action(operation) {
  try { await operation(); }
  catch (error) { toast(error.message || 'عملیات ناموفق بود.', true); }
}
function render(next) {
  state = next;
  $('connectionLabel').textContent = next.connected ? `متصل به ${next.account}` : 'بدون اتصال';
  $('connectionDot').classList.toggle('on', next.connected);
  $('heroStatus').textContent = next.status;
  $('armButton').disabled = !next.connected || !next.rules.length || next.armed;
  $('stopButton').disabled = !next.armed;
  $('armStatus').textContent = next.armed ? 'فعال' : 'متوقف';
  $('armStatus').style.color = next.armed ? 'var(--success)' : 'var(--text)';
  $('ruleCount').textContent = number(next.rules.length);
  $('lastPoll').textContent = next.lastPoll ? new Date(next.lastPoll).toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit' }) : '—';
  $('tokenInput').disabled = next.connected;
  $('connectButton').disabled = next.connected;
  $('connectButton').textContent = next.connected ? 'متصل شد' : 'اتصال';
  $('disconnectButton').hidden = !next.connected;
  [...$('ruleForm').elements].forEach((element) => { element.disabled = next.armed; });
  $('rulesList').innerHTML = next.rules.length ? next.rules.map((rule, index) => `<div class="rule-card"><span class="rule-number">${number(index + 1)}</span><div class="rule-info"><strong>${number(rule.min)} تا ${number(rule.max)} تومان</strong><small>بازهٔ دونیت</small></div><span class="rule-action">${escapeHtml(rule.key)}</span><span class="rule-duration">${number(rule.duration / 1000)} ثانیه</span><div class="rule-buttons"><button type="button" class="mini-button" data-test="${escapeHtml(rule.id)}" title="تست دستی" ${next.armed ? 'disabled' : ''}>تست</button><button type="button" class="mini-button delete" data-delete="${escapeHtml(rule.id)}" title="حذف قانون" ${next.armed ? 'disabled' : ''}>حذف</button></div></div>`).join('') : '<div class="rules-empty">هنوز قانونی ندارید. اولین بازه را پایین تعریف کنید.</div>';
  $('activityList').innerHTML = next.history.length ? next.history.map((item) => `<div class="event ${escapeHtml(item.kind)}"><span class="event-symbol">${({ success: '✓', error: '!', queued: '↗', skip: '−', info: 'i' })[item.kind] || '·'}</span><span class="event-message">${escapeHtml(item.message)}</span><span class="event-amount">${item.amount === null ? '' : `${number(item.amount)} تومان`}</span><time class="event-time">${new Date(item.time).toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit' })}</time></div>`).join('') : '<div class="empty-state"><span>◇</span><strong>هنوز رویدادی ثبت نشده</strong><p>پس از اتصال و شروع کنترل، فعالیت‌ها اینجا نمایش داده می‌شوند.</p></div>';
}
window.titano.onState(render);
action(async () => render(await window.titano.state()));
$('connectButton').addEventListener('click', () => action(async () => {
  const input = $('tokenInput');
  if (!input.value.trim()) { toast('توکن ریمیت را وارد کنید.', true); input.focus(); return; }
  const button = $('connectButton'); button.disabled = true; button.textContent = 'در حال بررسی...';
  try { render(await window.titano.connect(input.value.trim())); input.value = ''; toast('حساب ریمیت متصل شد.'); }
  finally { if (!state.connected) { button.disabled = false; button.textContent = 'اتصال'; } }
}));
$('tokenInput').addEventListener('keydown', (event) => { if (event.key === 'Enter') $('connectButton').click(); });
$('disconnectButton').addEventListener('click', () => action(async () => render(await window.titano.disconnect())));
$('armButton').addEventListener('click', () => action(async () => { render(await window.titano.start()); toast('کنترل زنده روشن شد.'); }));
$('stopButton').addEventListener('click', () => action(async () => { render(await window.titano.stop()); toast('کنترل متوقف شد.'); }));
$('ruleForm').addEventListener('submit', (event) => { event.preventDefault(); action(async () => {
  const form = event.currentTarget;
  const data = new FormData(form);
  const min = Number(data.get('min')), max = Number(data.get('max')), duration = Math.round(Number(data.get('duration')) * 1000);
  const rule = { id: crypto.randomUUID(), min, max, duration, key: data.get('key') };
  render(await window.titano.rules([...state.rules, rule]));
  form.reset(); toast('قانون ذخیره شد.');
}); });
$('rulesList').addEventListener('click', (event) => {
  const test = event.target.closest('[data-test]');
  const remove = event.target.closest('[data-delete]');
  if (test) action(async () => { toast('پنجره کوچک می‌شود؛ کلید روی برنامهٔ قبلیِ فعال فرستاده می‌شود.'); render(await window.titano.test(test.dataset.test)); });
  if (remove) action(async () => { render(await window.titano.rules(state.rules.filter((rule) => rule.id !== remove.dataset.delete))); toast('قانون حذف شد.'); });
});
document.querySelectorAll('[data-link]').forEach((button) => button.addEventListener('click', () => window.titano.link(button.dataset.link)));
document.querySelectorAll('.nav-item').forEach((link) => link.addEventListener('click', () => { document.querySelectorAll('.nav-item').forEach((item) => item.classList.remove('active')); link.classList.add('active'); }));
