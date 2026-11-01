'use strict';
const $ = (id) => document.getElementById(id);
const escapeHtml = (value) => String(value).replace(/[&<>"']/g, (char) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[char]);
const dictionaries = {
  fa: {
    appTitle: 'Titano Trigger — kiatitano & titanoteam', workspace: 'فضای کار', mainNavigation: 'ناوبری اصلی',
    'nav.overview': 'نمای کلی', 'nav.connection': 'اتصال درگاه', 'nav.rules': 'قوانین اجرا', 'nav.activity': 'رویدادها',
    'safety.title': 'کنترل در اختیار شماست', 'safety.shortcut': 'توقف فوری در هر لحظه با', createdBy: 'ساخته شده توسط',
    'language.label': 'زبان', 'language.aria': 'زبان رابط کاربری', appStatus: 'وضعیت برنامه',
    'page.title': 'استریم، با هر دونیت زنده‌تر.', 'page.subtitle': 'دونیت‌های ریمیت را به ورودی دلخواه کیبورد یا ماوس تبدیل کنید.',
    'hero.kicker': 'LIVE CONTROL ENGINE', 'hero.title': 'کنترل زنده،', 'hero.emphasis': 'دست شماست.', 'hero.start': 'شروع کنترل زنده', 'hero.stop': 'توقف کنترل', 'hero.disconnected': 'برای شروع، حساب ریمیت خود را وصل کنید.',
    'stats.run': 'وضعیت اجرا', 'stats.manual': 'فعال‌سازی دستی لازم است', 'stats.rules': 'قوانین فعال', 'stats.range': 'بازهٔ تومانی تعریف‌شده', 'stats.lastCheck': 'آخرین بررسی', 'stats.pollInterval': 'حدود هر ۲ ثانیه',
    disconnected: 'بدون اتصال', connectedTo: 'متصل به {name}', active: 'فعال', stopped: 'متوقف',
    'connection.title': 'اتصال درگاه', 'connection.subtitle': 'توکن فقط در حافظهٔ همین اجرای برنامه نگهداری می‌شود.',
    'reymit.name': 'ریمیت', 'reymit.official': 'اتصال رسمی API', 'provider.active': 'فعال', 'provider.connected': 'متصل شد',
    'token.label': 'توکن API ریمیت', 'token.placeholder': 'توکن را اینجا وارد کنید', 'token.disconnect': 'قطع اتصال', 'token.help': 'از تنظیمات حساب ریمیت ← متفرقه دریافت کنید. در فایل برنامه ذخیره نمی‌شود.',
    'donofa.name': 'دونوفا', 'donofa.waiting': 'در انتظار مستندات رسمی API', 'donofa.disabled': 'فعلاً غیرفعال', 'donofa.description': 'پس از انتشار مسیر معتبر دریافت دونیت برای کاربران، این درگاه اضافه می‌شود.',
    'rules.title': 'قوانین اجرا', 'rules.subtitle': 'برای هر بازهٔ مبلغ، یک کلید یا دکمهٔ ماوس و مدت نگه‌داشتن آن را تعیین کنید.', 'rules.unit': 'واحد: تومان', 'rules.new': 'قانون جدید',
    'rules.minimum': 'از مبلغ', 'rules.maximum': 'تا مبلغ', 'rules.minimumPlaceholder': '۱۰٬۰۰۰', 'rules.maximumPlaceholder': '۵۰٬۰۰۰', 'rules.key': 'کلید / ماوس', 'rules.duration': 'مدت (ثانیه)', 'rules.add': 'افزودن قانون', 'rules.footnote': 'بازه‌ها نباید هم‌پوشانی داشته باشند. حداکثر مدت هر فرمان ۱۰ ثانیه است.',
    'activity.title': 'رویدادهای اخیر', 'activity.subtitle': 'فقط دونیت‌های جدید پس از روشن‌کردن کنترل بررسی می‌شوند.', 'activity.live': 'رویداد زنده', 'activity.emptyTitle': 'هنوز رویدادی ثبت نشده', 'activity.emptyText': 'پس از اتصال و شروع کنترل، فعالیت‌ها اینجا نمایش داده می‌شوند.',
    'footer.api': 'راهنمای API ریمیت ↗',
    'connect.button': 'اتصال', 'connect.checking': 'در حال بررسی...', 'connect.connected': 'متصل شد', 'toast.connected': 'حساب ریمیت متصل شد.', 'toast.liveStarted': 'کنترل زنده روشن شد.', 'toast.stopped': 'کنترل متوقف شد.', 'toast.ruleSaved': 'قانون ذخیره شد.', 'toast.ruleDeleted': 'قانون حذف شد.', 'toast.test': 'پنجره کوچک می‌شود؛ کلید روی برنامهٔ قبلیِ فعال فرستاده می‌شود.',
    'error.operation': 'عملیات ناموفق بود.', 'error.enterToken': 'توکن ریمیت را وارد کنید.', 'error.noRules': 'هنوز قانونی ندارید. اولین بازه را پایین تعریف کنید.',
    'rule.donationRange': 'بازهٔ دونیت', 'rule.test': 'تست دستی', 'rule.delete': 'حذف قانون', 'unit.toman': 'تومان', 'unit.seconds': 'ثانیه',
    'empty.unknown': 'یک عملیات ناموفق بود. اتصال ریمیت را بررسی و دوباره تلاش کنید.',
    'key.MOUSE_LEFT': 'ماوس چپ', 'key.MOUSE_RIGHT': 'ماوس راست', 'key.MOUSE_MIDDLE': 'ماوس وسط'
  },
  en: {
    appTitle: 'Titano Trigger — kiatitano & titanoteam', workspace: 'WORKSPACE', mainNavigation: 'Main navigation',
    'nav.overview': 'Overview', 'nav.connection': 'Connect provider', 'nav.rules': 'Action rules', 'nav.activity': 'Activity',
    'safety.title': 'You are in control', 'safety.shortcut': 'Stop immediately at any time with', createdBy: 'Created by',
    'language.label': 'Language', 'language.aria': 'Interface language', appStatus: 'Application status',
    'page.title': 'Make every donation count.', 'page.subtitle': 'Turn new Reymit donations into keyboard or mouse actions.',
    'hero.kicker': 'LIVE CONTROL ENGINE', 'hero.title': 'Live control,', 'hero.emphasis': 'your rules.', 'hero.start': 'Start live control', 'hero.stop': 'Stop control', 'hero.disconnected': 'Connect your Reymit account to get started.',
    'stats.run': 'Control status', 'stats.manual': 'Manual activation required', 'stats.rules': 'Active rules', 'stats.range': 'Configured Toman ranges', 'stats.lastCheck': 'Last check', 'stats.pollInterval': 'Checks about every 2 seconds',
    disconnected: 'Not connected', connectedTo: 'Connected to {name}', active: 'Active', stopped: 'Stopped',
    'connection.title': 'Connect a provider', 'connection.subtitle': 'Your token stays in memory only while this app is running.',
    'reymit.name': 'Reymit', 'reymit.official': 'Official API connection', 'provider.active': 'Available', 'provider.connected': 'Connected',
    'token.label': 'Reymit API token', 'token.placeholder': 'Paste your token here', 'token.disconnect': 'Disconnect', 'token.help': 'Get a token from Reymit account settings → Miscellaneous. It is not saved in app files.',
    'donofa.name': 'Donofa', 'donofa.waiting': 'Awaiting official API documentation', 'donofa.disabled': 'Unavailable', 'donofa.description': 'This provider can be added after a supported donation API becomes available to users.',
    'rules.title': 'Action rules', 'rules.subtitle': 'Choose a keyboard key or mouse button and hold duration for each donation range.', 'rules.unit': 'Currency: Toman', 'rules.new': 'New rule',
    'rules.minimum': 'Minimum amount', 'rules.maximum': 'Maximum amount', 'rules.minimumPlaceholder': '10,000', 'rules.maximumPlaceholder': '50,000', 'rules.key': 'Key / mouse', 'rules.duration': 'Hold (seconds)', 'rules.add': 'Add rule', 'rules.footnote': 'Donation ranges cannot overlap. Each action can be held for up to 10 seconds.',
    'activity.title': 'Recent activity', 'activity.subtitle': 'Only donations received after live control starts are checked.', 'activity.live': 'Live activity', 'activity.emptyTitle': 'No activity yet', 'activity.emptyText': 'Events will appear here after you connect and start control.',
    'footer.api': 'Reymit API docs ↗',
    'connect.button': 'Connect', 'connect.checking': 'Checking...', 'connect.connected': 'Connected', 'toast.connected': 'Reymit account connected.', 'toast.liveStarted': 'Live control started.', 'toast.stopped': 'Control stopped.', 'toast.ruleSaved': 'Rule saved.', 'toast.ruleDeleted': 'Rule deleted.', 'toast.test': 'The app will minimize and send the key to the previously active window.',
    'error.operation': 'The operation failed.', 'error.enterToken': 'Enter your Reymit token.', 'error.noRules': 'No rules yet. Add your first donation range below.',
    'rule.donationRange': 'Donation range', 'rule.test': 'Test action', 'rule.delete': 'Delete rule', 'unit.toman': 'Toman', 'unit.seconds': 'sec',
    'empty.unknown': 'The operation failed. Check your Reymit connection and try again.',
    'key.MOUSE_LEFT': 'Mouse left', 'key.MOUSE_RIGHT': 'Mouse right', 'key.MOUSE_MIDDLE': 'Mouse middle'
  }
};

const backendMessages = {
  'برای شروع، توکن ریمیت را وصل کنید.': 'Connect your Reymit token to get started.',
  'در حال بررسی اتصال...': 'Checking the connection...',
  'ریمیت متصل است. برای اجرا، کنترل را روشن کنید.': 'Reymit is connected. Start live control when you are ready.',
  'ریمیت قطع شد.': 'Reymit disconnected.',
  'قوانین ذخیره شدند.': 'Rules saved.',
  'در حال پایش دونیت‌های ریمیت': 'Checking for new Reymit donations.',
  'کنترل روشن شد. دونیت‌های قبلی اجرا نمی‌شوند.': 'Live control started. Earlier donations will not trigger actions.',
  'صف فرمان‌ها پر است؛ این دونیت اجرا نشد.': 'The action queue is full. This donation was skipped.',
  'اتصال یا اجرای ورودی شکست خورد؛ کنترل متوقف شد.': 'The connection or input action failed. Control has stopped.',
  'اجرای ورودی شکست خورد؛ کنترل متوقف شد.': 'The input action failed. Control has stopped.',
  'کنترل متوقف شد.': 'Control stopped.',
  'تست فقط در حالت توقف مجاز است.': 'Test actions are available only while control is stopped.',
  'قانون پیدا نشد.': 'Rule not found.',
  'توکن ریمیت نامعتبر است.': 'The Reymit token is invalid.',
  'ریمیت اتصال را تأیید نکرد.': 'Reymit did not authorize the connection.',
  'پاسخ ریمیت شناخته نشد.': 'Reymit returned an unrecognized response.',
  'پاسخ حساب ریمیت شناخته نشد.': 'Reymit returned an unrecognized account response.',
  'فهرست دونیت ریمیت شناخته نشد.': 'Reymit returned an unrecognized donation list.',
  'ارتباط با ریمیت برقرار نشد؛ اتصال اینترنت را بررسی کنید.': 'Could not reach Reymit. Check your internet connection.',
  'حداکثر ۵۰ قانون مجاز است.': 'You can add up to 50 rules.',
  'برای این کلید scan code ویندوز پیدا نشد.': 'Windows could not find a scan code for this key.',
  'کلید مجاز نیست.': 'This key is not supported.',
  'دکمهٔ ماوس مجاز نیست.': 'This mouse button is not supported.',
  'یک فرمان در حال اجراست.': 'An action is already running.',
  'برای شروع، توکن ریمیت را وصل کنید.': 'Connect your Reymit token to get started.',
  'برنامه بسته شد.': 'The app was closed.',
  'توقف اضطراری با میانبر انجام شد.': 'Emergency stop triggered by the keyboard shortcut.',
  'درخواست ناشناخته است.': 'Unknown request.'
};

let language = 'fa';
let state = { connected: false, armed: false, rules: [], history: [] };
let toastTimer;
const $number = (value) => new Intl.NumberFormat(language === 'fa' ? 'fa-IR' : 'en-US', { maximumFractionDigits: 1 }).format(value);
const t = (key, values = {}) => Object.entries(values).reduce((text, [name, value]) => text.replaceAll(`{${name}}`, value), dictionaries[language][key] ?? key);
const keys = [...'ABCDEFGHIJKLMNOPQRSTUVWXYZ', ...'0123456789', ...Array.from({ length: 12 }, (_, i) => `F${i + 1}`), 'SPACE', 'ENTER', 'TAB', 'ESC', 'UP', 'DOWN', 'LEFT', 'RIGHT', 'MOUSE_LEFT', 'MOUSE_RIGHT', 'MOUSE_MIDDLE'];

function translateMessage(message) {
  if (language === 'fa' || !message) return message || t('error.operation');
  const direct = backendMessages[message];
  if (direct) return direct;
  let match = message.match(/^اتصال ریمیت برای (.+) تأیید شد\.$/u);
  if (match) return `Reymit connection verified for ${match[1]}.`;
  match = message.match(/^(.+) · خارج از بازه‌های تعریف‌شده$/u);
  if (match) return `${match[1]} · outside configured donation ranges`;
  match = message.match(/^(.+) · (.+) در صف اجرا$/u);
  if (match) return `${match[1]} · ${match[2]} queued`;
  match = message.match(/^(.+) · (.+) برای ([\d۰-۹٠-٩.,٫]+) ثانیه$/u);
  if (match) return `${match[1]} · ${match[2]} for ${normalizeDigits(match[3])} sec`;
  match = message.match(/^تست دستی (.+) انجام شد\.$/u);
  if (match) return `Manual test for ${match[1]} completed.`;
  match = message.match(/^پاسخ ریمیت: HTTP (\d+)$/u);
  if (match) return `Reymit response: HTTP ${match[1]}`;
  match = message.match(/^بازهٔ مبلغ قانون (\d+) معتبر نیست\.$/u);
  if (match) return `The amount range for rule ${match[1]} is invalid.`;
  match = message.match(/^مدت قانون (\d+) باید بین .+ باشد\.$/u);
  if (match) return `The hold duration for rule ${match[1]} must be between 0.1 and 10 seconds.`;
  match = message.match(/^کلید قانون (\d+) پشتیبانی نمی‌شود\.$/u);
  if (match) return `The key for rule ${match[1]} is not supported.`;
  match = message.match(/^شناسهٔ قانون (\d+) معتبر نیست\.$/u);
  if (match) return `The ID for rule ${match[1]} is invalid.`;
  if (message.startsWith('بازه‌های مبلغ نباید هم‌پوشانی داشته باشند')) return 'Donation amount ranges cannot overlap.';
  if (message.startsWith('برای ویرایش قوانین، کنترل را متوقف کنید')) return 'Stop control before editing rules.';
  if (message.startsWith('ابتدا ریمیت را وصل کنید')) return 'Connect Reymit first.';
  if (message.startsWith('حداقل یک قانون تعریف کنید')) return 'Add at least one rule.';
  if (message.startsWith('پایش یا اجرا ناموفق بود: ')) return `Polling or action failed: ${translateMessage(message.slice('پایش یا اجرا ناموفق بود: '.length))}`;
  if (message.startsWith('اجرای ورودی ناموفق بود: ')) return `Input action failed: ${translateMessage(message.slice('اجرای ورودی ناموفق بود: '.length))}`;
  if (message.startsWith('ویندوز ورودی را نپذیرفت')) {
    const code = message.match(/کد (\d+)/u)?.[1];
    return `Windows rejected simulated input${code ? ` (error ${code})` : ''}. If the target app runs as administrator, run Titano Trigger at the same privilege level.`;
  }
  return /[\u0600-\u06ff]/u.test(message) ? t('empty.unknown') : message;
}

function normalizeDigits(value) {
  return String(value).replace(/[۰-۹٠-٩]/g, (digit) => String('۰۱۲۳۴۵۶۷۸۹٠١٢٣٤٥٦٧٨٩'.indexOf(digit) % 10)).replace('٫', '.').replace(',', '.');
}

function applyLanguage() {
  document.documentElement.lang = language;
  document.documentElement.dir = language === 'fa' ? 'rtl' : 'ltr';
  document.title = t('appTitle');
  document.querySelectorAll('[data-i18n]').forEach((element) => { element.textContent = t(element.dataset.i18n); });
  document.querySelectorAll('[data-i18n-aria]').forEach((element) => { element.setAttribute('aria-label', t(element.dataset.i18nAria)); });
  document.querySelectorAll('[data-i18n-placeholder]').forEach((element) => { element.placeholder = t(element.dataset.i18nPlaceholder); });
  document.querySelectorAll('[data-i18n-title]').forEach((element) => { element.title = t(element.dataset.i18nTitle); });
  $('languageSelect').value = language;
  $('keySelect').innerHTML = keys.map((key) => `<option value="${key}">${escapeHtml(t(`key.${key}`) === `key.${key}` ? key : t(`key.${key}`))}</option>`).join('');
  if (state) render(state);
}

function toast(message, error = false) {
  const element = $('toast');
  element.textContent = message;
  element.className = `toast show${error ? ' error' : ''}`;
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => { element.className = 'toast'; }, 4500);
}

async function action(operation) {
  try { await operation(); }
  catch (error) { toast(translateMessage(error.message) || t('error.operation'), true); }
}

function emptyActivity() {
  return `<div class="empty-state"><span aria-hidden="true">◇</span><strong>${t('activity.emptyTitle')}</strong><p>${t('activity.emptyText')}</p></div>`;
}

function render(next) {
  state = next;
  $('connectionLabel').textContent = next.connected ? t('connectedTo', { name: next.account }) : t('disconnected');
  $('connectionDot').classList.toggle('on', next.connected);
  $('heroStatus').textContent = translateMessage(next.status) || (next.connected ? t('active') : t('hero.disconnected'));
  $('armButton').disabled = !next.connected || !next.rules.length || next.armed;
  $('stopButton').disabled = !next.armed;
  $('armStatus').textContent = next.armed ? t('active') : t('stopped');
  $('armStatus').style.color = next.armed ? 'var(--success)' : 'var(--text)';
  $('ruleCount').textContent = $number(next.rules.length);
  $('lastPoll').textContent = next.lastPoll ? new Date(next.lastPoll).toLocaleTimeString(language === 'fa' ? 'fa-IR' : 'en-US', { hour: '2-digit', minute: '2-digit' }) : '—';
  $('tokenInput').disabled = next.connected;
  $('connectButton').disabled = next.connected;
  $('connectButton').textContent = t(next.connected ? 'provider.connected' : 'connect.button');
  $('disconnectButton').hidden = !next.connected;
  [...$('ruleForm').elements].forEach((element) => { element.disabled = next.armed; });
  $('rulesList').innerHTML = next.rules.length ? next.rules.map((rule, index) => `<div class="rule-card"><span class="rule-number">${$number(index + 1)}</span><div class="rule-info"><strong>${$number(rule.min)}–${$number(rule.max)} ${t('unit.toman')}</strong><small>${t('rule.donationRange')}</small></div><span class="rule-action">${escapeHtml(t(`key.${rule.key}`) === `key.${rule.key}` ? rule.key : t(`key.${rule.key}`))}</span><span class="rule-duration">${$number(rule.duration / 1000)} ${t('unit.seconds')}</span><div class="rule-buttons"><button type="button" class="mini-button" data-test="${escapeHtml(rule.id)}" title="${t('rule.test')}" aria-label="${t('rule.test')} ${escapeHtml(rule.key)}" ${next.armed ? 'disabled' : ''}>${t('rule.test')}</button><button type="button" class="mini-button delete" data-delete="${escapeHtml(rule.id)}" title="${t('rule.delete')}" aria-label="${t('rule.delete')} ${escapeHtml(rule.key)}" ${next.armed ? 'disabled' : ''}>${t('rule.delete')}</button></div></div>`).join('') : `<div class="rules-empty">${t('error.noRules')}</div>`;
  $('activityList').innerHTML = next.history.length ? next.history.map((item) => `<div class="event ${escapeHtml(item.kind)}"><span class="event-symbol">${({ success: '✓', error: '!', queued: '↗', skip: '−', info: 'i' })[item.kind] || '·'}</span><span class="event-message">${escapeHtml(translateMessage(item.message))}</span><span class="event-amount">${item.amount === null ? '' : `${$number(item.amount)} ${t('unit.toman')}`}</span><time class="event-time">${new Date(item.time).toLocaleTimeString(language === 'fa' ? 'fa-IR' : 'en-US', { hour: '2-digit', minute: '2-digit' })}</time></div>`).join('') : emptyActivity();
}

window.titano.onState(render);
action(async () => {
  language = await window.titano.language(null);
  if (language !== 'en') language = 'fa';
  applyLanguage();
  render(await window.titano.state());
});

$('languageSelect').addEventListener('change', () => action(async () => {
  language = await window.titano.language($('languageSelect').value);
  applyLanguage();
}));

$('connectButton').addEventListener('click', () => action(async () => {
  const input = $('tokenInput');
  if (!input.value.trim()) { toast(t('error.enterToken'), true); input.focus(); return; }
  const button = $('connectButton'); button.disabled = true; button.textContent = t('connect.checking');
  try { render(await window.titano.connect(input.value.trim())); input.value = ''; toast(t('toast.connected')); }
  finally { if (!state.connected) { button.disabled = false; button.textContent = t('connect.button'); } }
}));
$('tokenInput').addEventListener('keydown', (event) => { if (event.key === 'Enter') $('connectButton').click(); });
$('disconnectButton').addEventListener('click', () => action(async () => render(await window.titano.disconnect())));
$('armButton').addEventListener('click', () => action(async () => { render(await window.titano.start()); toast(t('toast.liveStarted')); }));
$('stopButton').addEventListener('click', () => action(async () => { render(await window.titano.stop()); toast(t('toast.stopped')); }));
$('ruleForm').addEventListener('submit', (event) => { event.preventDefault(); action(async () => {
  const form = event.currentTarget;
  const data = new FormData(form);
  const min = Number(data.get('min')), max = Number(data.get('max')), duration = Math.round(Number(data.get('duration')) * 1000);
  const rule = { id: crypto.randomUUID(), min, max, duration, key: data.get('key') };
  render(await window.titano.rules([...state.rules, rule]));
  form.reset(); toast(t('toast.ruleSaved'));
}); });
$('rulesList').addEventListener('click', (event) => {
  const test = event.target.closest('[data-test]');
  const remove = event.target.closest('[data-delete]');
  if (test) action(async () => { toast(t('toast.test')); render(await window.titano.test(test.dataset.test)); });
  if (remove) action(async () => { render(await window.titano.rules(state.rules.filter((rule) => rule.id !== remove.dataset.delete))); toast(t('toast.ruleDeleted')); });
});
document.querySelectorAll('[data-link]').forEach((button) => button.addEventListener('click', () => window.titano.link(button.dataset.link)));
document.querySelectorAll('.nav-item').forEach((link) => link.addEventListener('click', () => { document.querySelectorAll('.nav-item').forEach((item) => item.classList.remove('active')); link.classList.add('active'); }));
