// ~/apps/usersettings.js
import { UserApi } from '../Entites/index.js';

/* ============================================================
   PBKDF2 (SHA-256) yardımcıları
   Format: PBKDF2$<iter>$<saltB64>$<hashB64>
   ============================================================ */
const PBKDF2_ITER = 100000;
const SALT_LEN = 16;   // bytes
const DK_LEN = 32;     // bytes (256-bit)

function toBase64(data) {
    const bytes = data instanceof ArrayBuffer ? new Uint8Array(data)
        : data instanceof Uint8Array ? data
            : new Uint8Array(data);
    let bin = '';
    for (let i = 0; i < bytes.length; i++) bin += String.fromCharCode(bytes[i]);
    return btoa(bin);
}

async function makePasswordHash(password) {
    if (!window.crypto || !window.crypto.subtle) {
        throw new Error('Tarayıcınız WebCrypto (PBKDF2) desteklemiyor.');
    }

    const salt = new Uint8Array(SALT_LEN);
    window.crypto.getRandomValues(salt);

    const enc = new TextEncoder();
    const key = await window.crypto.subtle.importKey(
        'raw',
        enc.encode(password),
        { name: 'PBKDF2' },
        false,
        ['deriveBits']
    );

    const bits = await window.crypto.subtle.deriveBits(
        { name: 'PBKDF2', hash: 'SHA-256', salt, iterations: PBKDF2_ITER },
        key,
        DK_LEN * 8
    );

    const saltB64 = toBase64(salt);
    const dkB64 = toBase64(bits);
    return `PBKDF2$${PBKDF2_ITER}$${saltB64}$${dkB64}`;
}

/* ============================================================
   RowVersion yardımcıları (User.js ile aynı mantık)
   ============================================================ */
function rowVersionHexToBase64(hex) {
    if (!hex) return "";
    let h = String(hex).trim();
    if (h.startsWith("0x") || h.startsWith("0X")) {
        h = h.substring(2);
    }
    if (h.length % 2 === 1) {
        h = "0" + h;
    }
    const bytes = [];
    for (let i = 0; i < h.length; i += 2) {
        const b = parseInt(h.substr(i, 2), 16);
        if (!isNaN(b)) bytes.push(b);
    }
    let bin = "";
    for (let i = 0; i < bytes.length; i++) {
        bin += String.fromCharCode(bytes[i]);
    }
    try {
        return btoa(bin);
    } catch (e) {
        console.error("RowVersion base64 dönüşümünde hata:", e);
        return "";
    }
}

function getRowVersionB64FromEntity(entity) {
    if (!entity) return "";
    let rv =
        entity.rowVersionBase64 || entity.RowVersionBase64 ||
        entity.rowVersion || entity.RowVersion ||
        entity.rowVersionHex || entity.RowVersionHex;

    if (rv && /^0x/i.test(rv)) {
        rv = rowVersionHexToBase64(rv);
    }
    return rv || "";
}

/* ============================================================
   Genel yardımcılar
   ============================================================ */
function toStr(v) { return (v === undefined || v === null) ? '' : (v + ''); }
function isValidEmail(e) { return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(toStr(e)); }
function escapeHtml(s) {
    return toStr(s).replace(/[&<>"']/g, function (m) {
        return ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[m];
    });
}
function onlyDigits(s) { return toStr(s).replace(/\D/g, ''); }

async function httpJson(url, options) {
    const opts = Object.assign({
        headers: { 'Content-Type': 'application/json; charset=utf-8' }
    }, options || {});

    const res = await fetch(url, opts);
    if (!res.ok) {
        const txt = await res.text();
        throw new Error(txt || res.statusText);
    }
    const ct = res.headers.get('Content-Type') || '';
    if (ct.indexOf('application/json') >= 0)
        return await res.json();
    return await res.text();
}

/* ============================================================
   GLOBAL (Layout -> window.*)
   ============================================================ */
const currentUserId = (typeof window.currentUserId !== 'undefined')
    ? Number(window.currentUserId)
    : null;

let currentUser = null;

let mailAdres = (typeof window.mailAdres !== 'undefined') ? window.mailAdres : '';
let telefon = (typeof window.telefon !== 'undefined') ? window.telefon : '';
let telefonDogrulandi = (typeof window.telefonDogrulandi !== 'undefined') ? window.telefonDogrulandi : false;
let mailAdresDogrulandi = (typeof window.mailAdresDogrulandi !== 'undefined') ? window.mailAdresDogrulandi : false;

/* ============================================================
   UI HELPERS
   ============================================================ */
function setVerifyUI(channel, verified) {
    if (channel === 'Mail') {
        $('#mailDogrulandidiv').toggle(!!verified);
        $('#mailDogrulanmadidiv').toggle(!verified);
    } else if (channel === 'Phone') {
        $('#telefonDogrulandidiv').toggle(!!verified);
        $('#telefonDogrulanmadidiv').toggle(!verified);
    }
}

function applyMasks() {
    if ($.fn.inputmask) {
        $('#txtkullanicitelefon, #phoneNum').inputmask('999 999-99-99', {
            placeholder: '___ ___-__-__',
            showMaskOnHover: false
        });
    }
}
function initSelect2() {
    if ($.fn.select2) {
        try { $('#konutipid').select2('destroy'); } catch (e) { }
        $('#konutipid').select2({ width: '100%' });
    }
}

function hydrateUserForm() {
    $('#txtkullanicimail').val(toStr(mailAdres || ''));

    let telPretty = toStr(telefon || '');
    if (telPretty && telPretty.length === 10)
        telPretty = telPretty.replace(/(\d{3})(\d{3})(\d{2})(\d{2})/, '$1 $2-$3-$4');
    $('#txtkullanicitelefon').val(telPretty);

    setVerifyUI('Mail', !!mailAdresDogrulandi);
    setVerifyUI('Phone', !!telefonDogrulandi);
}

/* ============================================================
   USER YÜKLEME (UserApi ile)
   ============================================================ */
async function loadCurrentUser() {
    if (!currentUserId) {
        // currentUserId yoksa, sadece window.* değerleri ile doldur
        hydrateUserForm();
        return;
    }

    try {
        const u = await UserApi.get(currentUserId);
        currentUser = u;

        const email = u.email ?? u.Email ?? mailAdres ?? '';
        const phone = u.phone ?? u.Phone ?? telefon ?? '';
        mailAdres = email;
        telefon = onlyDigits(phone);

        mailAdresDogrulandi = (u.emailVerified ?? u.EmailVerified ?? mailAdresDogrulandi);
        telefonDogrulandi = (u.phoneVerified ?? u.PhoneVerified ?? telefonDogrulandi);

        hydrateUserForm();
    } catch (err) {
        console.error('Kullanıcı bilgileri alınamadı:', err);
        hydrateUserForm(); // fallback
    }
}

/* ============================================================
   PASSWORD ALANI
   ============================================================ */
function yeniSifreAktif() {
    $('#sifreDegistirmeAktif').slideToggle(150);
    if (!$('#sifreDegistirmeAktif').is(':visible')) {
        $('#txteskisifre, #txtyenisifre, #txtyenisifretekrar').val('');
    }
}
function validatePasswordIfVisible() {
    if (!$('#sifreDegistirmeAktif').is(':visible')) return true;
    const oldP = $('#txteskisifre').val();
    const p1 = $('#txtyenisifre').val();
    const p2 = $('#txtyenisifretekrar').val();

    if (!oldP || !p1 || !p2) { if (window.toastr) toastr.error('Şifre alanları boş olamaz.'); return false; }
    if (p1.length < 8) { if (window.toastr) toastr.error('Yeni şifre en az 8 karakter olmalı.'); return false; }
    if (p1 !== p2) { if (window.toastr) toastr.error('Yeni şifre ile tekrar aynı olmalı.'); return false; }
    if (p1 === oldP) { if (window.toastr) toastr.error('Yeni şifre mevcut şifre ile aynı olamaz.'); return false; }
    return true;
}

/* ============================================================
   FIELD CHANGES
   ============================================================ */
function telefonChange() {
    const raw = $('#txtkullanicitelefon').val();
    telefon = onlyDigits(raw);
    telefonDogrulandi = false;
    setVerifyUI('Phone', false);
}
function emailChange() {
    mailAdres = $('#txtkullanicimail').val().trim();
    mailAdresDogrulandi = false;
    setVerifyUI('Mail', false);
}

/* ============================================================
   KULLANICI GÜNCELLEME (UserApi.update)
   ============================================================ */
async function updateUserByApi() {
    if (!currentUserId || !currentUser) {
        if (window.toastr) toastr.error('Kullanıcı bilgisi yüklenemedi.');
        return;
    }

    const newEmail = $('#txtkullanicimail').val().trim();
    const newPhone = onlyDigits($('#txtkullanicitelefon').val());

    const changePassword = $('#sifreDegistirmeAktif').is(':visible');
    const oldPassword = $('#txteskisifre').val();
    const newPasswordPlain = $('#txtyenisifre').val();

    const id = currentUser.id || currentUser.Id;
    const username = currentUser.username || currentUser.Username || '';

    const dto = {
        Id: id,
        Username: username,
        Email: newEmail,
        Phone: newPhone,
        IsActive: (currentUser.isActive !== undefined
            ? currentUser.isActive
            : (currentUser.IsActive !== undefined ? currentUser.IsActive : true)),
        EmailVerified: !!mailAdresDogrulandi,
        PhoneVerified: !!telefonDogrulandi
    };

    const rvB64 = getRowVersionB64FromEntity(currentUser);
    if (rvB64) dto.RowVersion = rvB64;

    if (changePassword && newPasswordPlain) {
        // 👇 Yeni parola için PBKDF2 hash
        dto.PasswordHash = await makePasswordHash(newPasswordPlain);
        // Eski şifreyi de backend istersen doğrulama için kullanabilir:
        dto.OldPassword = oldPassword;
    }

    const $btnUpd = $('#btn_guncelle').prop('disabled', true);

    try {
        const updated = await UserApi.update(dto.Id, dto);
        currentUser = updated || currentUser;

        mailAdres = newEmail;
        telefon = newPhone;

        // Yeni RowVersion varsa tekrar okuruz
        mailAdresDogrulandi = currentUser.emailVerified ?? currentUser.EmailVerified ?? mailAdresDogrulandi;
        telefonDogrulandi = currentUser.phoneVerified ?? currentUser.PhoneVerified ?? telefonDogrulandi;

        hydrateUserForm();
        $('#sifreDegistirmeAktif').hide();
        $('#txteskisifre, #txtyenisifre, #txtyenisifretekrar').val('');

        if (window.toastr) toastr.success('Bilgileriniz güncellendi.');
    } catch (err) {
        console.error(err);
        if (window.toastr) toastr.error(err.message || 'Güncelleme başarısız.');
    } finally {
        $btnUpd.prop('disabled', false);
    }
}

/* ============================================================
   MAIL / TELEFON DOĞRULAMA KODU GÖNDERME
   (şimdilik mevcut /User endpointlerini kullanıyoruz)
   ============================================================ */
async function changeUserInfos(action) {
    try {
        if (action === 'Mail') {
            const email = $('#txtkullanicimail').val().trim();
            if (!isValidEmail(email)) {
                if (window.toastr) toastr.error('Geçerli bir e-posta giriniz.');
                return;
            }

            const $btn = $('#mailDogrulamagonder').prop('disabled', true);
            await httpJson('/User/SendMailVerification', {
                method: 'POST',
                body: JSON.stringify({ email: email })
            });
            if (window.toastr) toastr.success('Doğrulama kodu e-postanıza gönderildi.');
            $btn.prop('disabled', false);
            return;
        }

        if (action === 'Phone') {
            const digits = onlyDigits($('#txtkullanicitelefon').val());
            if (digits.length < 10) {
                if (window.toastr) toastr.error('Geçerli bir telefon giriniz.');
                return;
            }

            const $btn2 = $('#telDogrulamagonder').prop('disabled', true);
            await httpJson('/User/SendPhoneVerification', {
                method: 'POST',
                body: JSON.stringify({ phone: digits })
            });
            if (window.toastr) toastr.success('SMS doğrulama kodu gönderildi.');
            $btn2.prop('disabled', false);
            return;
        }

        // Güncelle + (varsa) şifre değiştir (UserApi ile)
        if (!validatePasswordIfVisible()) return;
        await updateUserByApi();

    } catch (err) {
        console.error(err);
        if (window.toastr) toastr.error('İşlem sırasında hata oluştu.');
        $('#mailDogrulamagonder, #telDogrulamagonder, #btn_guncelle').prop('disabled', false);
    }
}

/* ============================================================
   MESAJ / TALEP (Support)
   ============================================================ */
function openMessageModal() {
    $('#message-modal').modal('show');
    loadMessages();
}

async function loadMessages() {
    $('#messagesAll').html('<div class="text-muted">Yükleniyor...</div>');
    try {
        const list = await httpJson('/Support/ListTickets', { method: 'GET' });

        if (!list || !list.length) {
            $('#messagesAll').html('<div class="alert alert-info">Kayıt bulunamadı.</div>');
            return;
        }

        const html = list.map(m => (
            '<div class="callout callout-default" style="margin-bottom:12px">' +
            '<b>' + escapeHtml(m.konu || '') + '</b>' +
            '<div>' + escapeHtml(m.aciklama || '') + '</div>' +
            '<small>' + escapeHtml(m.tarih || '') + ' • ' + escapeHtml(m.durum || '') + '</small>' +
            '</div>'
        )).join('');

        $('#messagesAll').html(html);
    } catch (err) {
        console.error(err);
        $('#messagesAll').html('<div class="alert alert-warning">Mesajlar alınamadı.</div>');
    }
}

async function sendTalep() {
    const data = {
        konuTipId: +($('#konutipid').val() || 0),
        konu: $('#konutxt').val().trim(),
        aciklama: $('#aciklama').val().trim(),
        phone: onlyDigits($('#phoneNum').val()),
        email: $('#email').val().trim()
    };

    if (!data.konu) { if (window.toastr) toastr.error('Konu alanı zorunludur.'); return; }
    if (!data.aciklama) { if (window.toastr) toastr.error('Açıklama alanı zorunludur.'); return; }
    if (!isValidEmail(data.email)) { if (window.toastr) toastr.error('Geçerli bir e-posta giriniz.'); return; }
    if ((data.phone || '').length < 10) { if (window.toastr) toastr.error('Geçerli bir telefon giriniz.'); return; }

    const $btn = $('#btnSendTalep').prop('disabled', true);

    try {
        await httpJson('/Support/SendTicket', {
            method: 'POST',
            body: JSON.stringify(data)
        });

        if (window.toastr) toastr.success('İstek/Talep gönderildi.');
        $('#konutxt').val('');
        $('#aciklama').val('');
        loadMessages();
    } catch (err) {
        console.error(err);
        if (window.toastr) toastr.error('İstek/Talep gönderilemedi.');
    } finally {
        $btn.prop('disabled', false);
    }
}

/* ============================================================
   BEKLEYEN İNDİRMELER (Downloads)
   ============================================================ */
function openDownloadExcelModal() {
    $('#downloadExcel-modal').modal('show');
    refreshDownloadTable();
}

async function refreshDownloadTable() {
    $('#excelTable').html('<div class="text-muted">Yükleniyor...</div>');

    try {
        const items = await httpJson('/Downloads/Pending', { method: 'GET' });

        if (!items || !items.length) {
            $('#excelTable').html('<div class="alert alert-info">Bekleyen indirmeniz yok.</div>');
            return;
        }

        const rows = items.map(it => {
            const link = it.url
                ? `<a class="btn btn-xs btn-success" href="${escapeHtml(it.url)}" target="_blank">İndir</a>`
                : '<span class="text-muted">Hazırlanıyor</span>';

            return (
                '<tr>' +
                '<td>' + escapeHtml(it.name || '') + '</td>' +
                '<td>' + escapeHtml(it.size || '') + '</td>' +
                '<td>' + escapeHtml(it.status || 'Hazır') + '</td>' +
                '<td style="width:120px">' + link + '</td>' +
                '</tr>'
            );
        }).join('');

        const tableHtml =
            '<table class="table table-bordered">' +
            '<thead>' +
            '<tr><th>Dosya</th><th>Boyut</th><th>Durum</th><th>İşlem</th></tr>' +
            '</thead>' +
            '<tbody>' + rows + '</tbody>' +
            '</table>';

        $('#excelTable').html(tableHtml);
    } catch (err) {
        console.error(err);
        $('#excelTable').html('<div class="alert alert-warning">Liste yüklenemedi.</div>');
    }
}

/* ============================================================
   INIT & EVENTS
   ============================================================ */
$(function () {
    (async () => {
        applyMasks();
        initSelect2();
        await loadCurrentUser();

        // events
        $('#sifreDegistirBtn').on('click', yeniSifreAktif);
        $('#txtkullanicimail').on('input change', emailChange);
        $('#txtkullanicitelefon').on('input change', telefonChange);

        $('#mailDogrulamagonder').on('click', function () { changeUserInfos('Mail'); });
        $('#telDogrulamagonder').on('click', function () { changeUserInfos('Phone'); });
        $('#btn_guncelle').on('click', function () { changeUserInfos(''); });

        $(document).on('click', '.jsOpenMessageModal', openMessageModal);
        $(document).on('click', '.jsOpenDownloads', openDownloadExcelModal);

        $('#btnSendTalep').on('click', sendTalep);
    })();
});
