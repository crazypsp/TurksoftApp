import { BankStatementsApi } from '../entities/index.js';

const $ = s => document.querySelector(s);
const val = el => (el?.value ?? '').trim();
const LS_KEY = (bankId) => `bank-cred-${bankId}`;

let bankData = [];
let dt = null;

function setStatus(msg) {
    const el = $('#lblStatus');
    if (el) el.textContent = msg || '';
}

function toIsoRange(dateStr, isEnd = false) {
    if (!dateStr) return null;
    return isEnd ? `${dateStr}T23:59:59.000Z` : `${dateStr}T00:00:00.000Z`;
}

function safeParseJson(text) {
    const t = (text || '').trim();
    if (!t) return {};
    try { return JSON.parse(t); }
    catch { throw new Error('Extras JSON geçersiz.'); }
}

function loadCred(bankId) {
    try {
        const raw = localStorage.getItem(LS_KEY(bankId));
        return raw ? JSON.parse(raw) : null;
    } catch { return null; }
}

function saveCred(bankId, obj) {
    localStorage.setItem(LS_KEY(bankId), JSON.stringify(obj || {}));
}

function fillSelect(selectEl, items, getText, getValue, placeholder = 'Seçiniz...') {
    if (!selectEl) return;

    selectEl.innerHTML = '';
    const opt0 = document.createElement('option');
    opt0.value = '';
    opt0.textContent = placeholder;
    selectEl.appendChild(opt0);

    (items || []).forEach(it => {
        const o = document.createElement('option');
        o.value = getValue(it);
        o.textContent = getText(it);
        selectEl.appendChild(o);
    });
}

/** ✅ API response her türlü array'e normalize */
function normalizeApiRows(payload) {
    if (!payload) return [];
    if (Array.isArray(payload)) return payload;

    // .NET ReferenceHandler.Preserve => { "$id":"1", "$values":[...] }
    if (Array.isArray(payload.$values)) return payload.$values;

    // bazı API’ler { value:[...] } veya { data:[...] } döner
    if (Array.isArray(payload.value)) return payload.value;
    if (Array.isArray(payload.data)) return payload.data;

    // tek obje gelmişse
    if (typeof payload === 'object') return [payload];

    return [];
}

/** ✅ key casing farkını tolere et */
function pick(row, ...keys) {
    if (!row) return '';
    for (const k of keys) {
        if (row[k] != null) return row[k];
    }
    // case-insensitive arama
    const lowerMap = Object.create(null);
    for (const kk of Object.keys(row)) lowerMap[kk.toLowerCase()] = kk;
    for (const k of keys) {
        const real = lowerMap[String(k).toLowerCase()];
        if (real && row[real] != null) return row[real];
    }
    return '';
}

function renderTable(rows) {
    const tableEl = $('#tblStatements');
    if (!tableEl) return;

    const data = normalizeApiRows(rows);

    if (typeof DataTable === 'undefined') {
        console.error('DataTable kütüphanesi yüklenmemiş. (datatables js/css kontrol et)');
        alert('DataTable kütüphanesi yüklenmemiş. Console’a bak.');
        return;
    }

    // yeniden init
    if (dt) { dt.destroy(); dt = null; tableEl.innerHTML = ''; }

    dt = new DataTable(tableEl, {
        data,
        searching: true,
        paging: true,
        pageLength: 25,
        order: [[0, 'desc']],
        columns: [
            { title: 'Tarih', data: r => pick(r, 'PROCESSTIMESTR', 'processTimeStr', 'processtimstr'), defaultContent: '' },
            { title: 'Hesap', data: r => pick(r, 'HESAPNO', 'hesapNo', 'accountNumber'), defaultContent: '' },
            { title: 'Açıklama', data: r => pick(r, 'PROCESSDESC', 'processDesc', 'description'), defaultContent: '' },
            { title: 'B/A', data: r => pick(r, 'PROCESSDEBORCRED', 'processDebOrCred', 'debitCredit'), defaultContent: '' },
            { title: 'Tutar', data: r => pick(r, 'PROCESSAMAOUNT', 'processAmount', 'amount'), defaultContent: '' },
            { title: 'Bakiye', data: r => pick(r, 'PROCESSBALANCE', 'processBalance', 'balance'), defaultContent: '' },
            { title: 'Ref', data: r => pick(r, 'PROCESSREFNO', 'processRefNo', 'refNo'), defaultContent: '' }
        ]
    });

    setStatus(`Kayıt: ${data.length}`);
}

async function loadBankAccounts() {
    const res = await fetch('/data/bankAccounts.json', { cache: 'no-store' });
    if (!res.ok) throw new Error('bankAccounts.json okunamadı.');
    bankData = await res.json();
}

function getSelectedBank() {
    const bankId = Number(val($('#selBank')));
    if (!bankId) return null;
    return bankData.find(x => Number(x.bankId) === bankId) || null;
}

function getSelectedAccountRaw() {
    const a = val($('#selAccount'));
    return a || null;
}

function getJsonDefaultCredentials(bank) {
    const creds =
        bank?.defaults?.credentials ||
        bank?.credentialFields?.credentials ||
        bank?.credentialFields ||
        {};

    const username =
        creds.username ??
        creds.userName ??
        creds.uid ??
        creds.usernameLabel ??
        '';

    const password =
        creds.password ??
        creds.pwd ??
        creds.pass ??
        creds.passwordLabel ??
        '';

    return {
        username: String(username || '').trim(),
        password: String(password || '').trim()
    };
}

function getEffectiveCred(bank) {
    const saved = loadCred(bank.bankId) || {};
    const defaults = bank?.defaults || {};
    const jsonCred = getJsonDefaultCredentials(bank);

    const username = (saved.username ?? jsonCred.username ?? '').trim();
    const password = (saved.password ?? jsonCred.password ?? '').trim();

    const link = (saved.link != null ? saved.link : defaults.link) ?? null;
    const tLink = (saved.tLink != null ? saved.tLink : defaults.tLink) ?? null;

    const extras = {
        ...(defaults.extras || {}),
        ...(saved.extras || {})
    };

    return { username, password, link, tLink, extras };
}

function mergeAccountMetaIntoExtras(selectedBank, selectedAccountNumber, baseExtras) {
    const extras = { ...(baseExtras || {}) };
    const acc = (selectedBank?.accounts || []).find(a => String(a.accountNumber) === String(selectedAccountNumber));
    if (!acc) return extras;

    const setIfEmpty = (k, v) => {
        if (v == null || v === '') return;
        if (extras[k] == null || extras[k] === '') extras[k] = v;
    };

    setIfEmpty('iban', acc.iban);
    setIfEmpty('IBAN', acc.iban);
    setIfEmpty('subeNo', acc.subeNo);
    setIfEmpty('SubeNo', acc.subeNo);
    setIfEmpty('musteriNo', acc.musteriNo);
    setIfEmpty('MusteriNo', acc.musteriNo);

    return extras;
}

function buildBasicFromUserPass(username, password) {
    const raw = `${username}:${password}`;
    const b64 = btoa(unescape(encodeURIComponent(raw)));
    return `Basic ${b64}`;
}

function normalizeZiraatKatilimExtras(extras, username, password) {
    const e = { ...(extras || {}) };

    if (!e.associationCode) throw new Error('Ziraat Katılım için extras.associationCode zorunlu.');

    const a = String(e.authorization || '').trim();
    if (!a) {
        if (!username || !password) throw new Error('Ziraat Katılım için username/password boş; authorization üretilemedi.');
        e.authorization = buildBasicFromUserPass(username, password);
        return e;
    }

    if (!/^basic\s+/i.test(a)) e.authorization = `Basic ${a}`;
    else e.authorization = a;

    return e;
}

function openCredModal(bankId) {
    const modalEl = $('#mdlCred');
    const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
    if (!modal) return;

    fillSelect($('#credBank'), bankData, x => x.bankName, x => x.bankId, 'Banka seç...');
    $('#credBank').value = String(bankId || '');

    const bank = bankId ? bankData.find(x => Number(x.bankId) === Number(bankId)) : null;
    const eff = bank ? getEffectiveCred(bank) : { username: '', password: '', link: '', tLink: '', extras: {} };

    $('#credUser').value = eff.username || '';
    $('#credPass').value = eff.password || '';
    $('#credLink').value = eff.link || bank?.defaults?.link || '';
    $('#credTLink').value = eff.tLink || bank?.defaults?.tLink || '';
    $('#credExtras').value = JSON.stringify(eff.extras || {}, null, 2);

    modal.show();
}

function wireUi() {
    const selBank = $('#selBank');
    const selAccount = $('#selAccount');

    fillSelect(selBank, bankData, x => x.bankName, x => x.bankId, 'Banka seç...');

    selBank?.addEventListener('change', () => {
        const bank = getSelectedBank();
        const accounts = bank?.accounts || [];

        if (accounts.length) {
            fillSelect(
                selAccount,
                accounts,
                x => `${x.accountNumber}${x.currency ? ` (${x.currency})` : ''}`,
                x => x.accountNumber,
                'Hesap seç...'
            );
        } else {
            fillSelect(
                selAccount,
                [{ accountNumber: '__manual__', currency: '' }],
                _ => 'Manuel hesap gir...',
                x => x.accountNumber,
                'Hesap seç...'
            );
        }
    });

    $('#btnCred')?.addEventListener('click', () => {
        const bank = getSelectedBank();
        openCredModal(bank?.bankId);
    });

    $('#frmCred')?.addEventListener('submit', (e) => {
        e.preventDefault();
        const bankId = Number(val($('#credBank')));
        if (!bankId) return alert('Banka seçiniz.');

        let extrasObj = {};
        try { extrasObj = safeParseJson($('#credExtras').value); }
        catch (err) { return alert(err.message); }

        const obj = {
            username: val($('#credUser')),
            password: val($('#credPass')),
            link: val($('#credLink')) || null,
            tLink: val($('#credTLink')) || null,
            extras: extrasObj
        };

        saveCred(bankId, obj);
        alert('Kaydedildi.');
        bootstrap.Modal.getInstance($('#mdlCred'))?.hide();
    });

    $('#btnFetch')?.addEventListener('click', async () => {
        const bank = getSelectedBank();
        if (!bank) return alert('Banka seçiniz.');

        let accountNumber = getSelectedAccountRaw();
        if (!accountNumber) return alert('Hesap seçiniz.');
        if (accountNumber === '__manual__') {
            accountNumber = prompt('Hesap numarasını giriniz:')?.trim() || '';
            if (!accountNumber) return alert('Hesap numarası girilmedi.');
        }

        const begin = val($('#dtBegin'));
        const end = val($('#dtEnd'));
        if (!begin || !end) return alert('Başlangıç ve bitiş tarihi seçiniz.');

        const cred = getEffectiveCred(bank);

        if (!cred.username || !cred.password) {
            openCredModal(bank.bankId);
            return alert('Bu banka için kullanıcı/parola boş. Modal’dan doldurun/kaydedin.');
        }

        if (bank?.required?.link && !cred.link) {
            openCredModal(bank.bankId);
            return alert('Bu banka için link zorunlu.');
        }
        if (bank?.required?.tLink && !cred.tLink) {
            openCredModal(bank.bankId);
            return alert('Bu banka için tLink zorunlu.');
        }

        let mergedExtras = mergeAccountMetaIntoExtras(bank, accountNumber, cred.extras);

        if (Number(bank.bankId) === 11) {
            mergedExtras = normalizeZiraatKatilimExtras(mergedExtras, cred.username, cred.password);
        }

        const dto = {
            bankId: Number(bank.bankId),
            username: cred.username,
            password: cred.password,
            accountNumber,
            beginDate: toIsoRange(begin, false),
            endDate: toIsoRange(end, true),
            link: cred.link || null,
            tLink: cred.tLink || null,
            extras: mergedExtras || {}
        };

        setStatus('İstek atılıyor...');
        try {
            const resp = await BankStatementsApi.statement(dto);

            // ✅ burada response normalize ediliyor
            const rows = normalizeApiRows(resp);

            renderTable(rows);
        } catch (err) {
            console.error(err);
            alert(err?.message || 'İstek başarısız.');
            setStatus('Hata');
        }
    });

    $('#btnClear')?.addEventListener('click', () => {
        renderTable([]);
        setStatus('');
    });
}

document.addEventListener('DOMContentLoaded', async () => {
    try {
        await loadBankAccounts();
        wireUi();

        const now = new Date();
        const y = now.getFullYear();
        const m = String(now.getMonth() + 1).padStart(2, '0');
        const lastDay = String(new Date(y, now.getMonth() + 1, 0).getDate()).padStart(2, '0');

        if ($('#dtBegin')) $('#dtBegin').value = `${y}-${m}-01`;
        if ($('#dtEnd')) $('#dtEnd').value = `${y}-${m}-${lastDay}`;
    } catch (e) {
        console.error(e);
        alert(e?.message || 'Sayfa başlatılamadı.');
    }
});
