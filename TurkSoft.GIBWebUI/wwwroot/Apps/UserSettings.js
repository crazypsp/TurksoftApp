
/* ====== UTIL ====== */
function toStr(v) { return (v === undefined || v === null) ? '' : (v + ''); }
function isValidEmail(e) { return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(toStr(e)); }
function escapeHtml(s) { return toStr(s).replace(/[&<>"']/g, function (m) { return ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[m]; }); }
function onlyDigits(s) { return toStr(s).replace(/\D/g, ''); }

/* ====== GLOBAL (layout tan geliyor olabilir) ====== */
var mailAdres = (typeof window.mailAdres !== 'undefined') ? window.mailAdres : '';
var telefon = (typeof window.telefon !== 'undefined') ? window.telefon : '';
var telefonDogrulandi = (typeof window.telefonDogrulandi !== 'undefined') ? window.telefonDogrulandi : false;
var mailAdresDogrulandi = (typeof window.mailAdresDogrulandi !== 'undefined') ? window.mailAdresDogrulandi : false;

/* ====== UI HELPERS ====== */
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
        $('#txtkullanicitelefon, #phoneNum').inputmask('999 999-99-99', { placeholder: '___ ___-__-__', showMaskOnHover: false });
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

    var telPretty = toStr(telefon || '');
    if (telPretty && telPretty.length === 10)
        telPretty = telPretty.replace(/(\d{3})(\d{3})(\d{2})(\d{2})/, '$1 $2-$3-$4');
    $('#txtkullanicitelefon').val(telPretty);

    setVerifyUI('Mail', !!mailAdresDogrulandi);
    setVerifyUI('Phone', !!telefonDogrulandi);
}

/* ====== PASSWORD ====== */
function yeniSifreAktif() {
    $('#sifreDegistirmeAktif').slideToggle(150);
    if (!$('#sifreDegistirmeAktif').is(':visible')) {
        $('#txteskisifre, #txtyenisifre, #txtyenisifretekrar').val('');
    }
}
function validatePasswordIfVisible() {
    if (!$('#sifreDegistirmeAktif').is(':visible')) return true;
    var oldP = $('#txteskisifre').val();
    var p1 = $('#txtyenisifre').val();
    var p2 = $('#txtyenisifretekrar').val();

    if (!oldP || !p1 || !p2) { if (window.toastr) toastr.error('Şifre alanları boş olamaz.'); return false; }
    if (p1.length < 8) { if (window.toastr) toastr.error('Yeni şifre en az 8 karakter olmalı.'); return false; }
    if (p1 !== p2) { if (window.toastr) toastr.error('Yeni şifre ile tekrar aynı olmalı.'); return false; }
    if (p1 === oldP) { if (window.toastr) toastr.error('Yeni şifre mevcut şifre ile aynı olamaz.'); return false; }
    return true;
}

/* ====== FIELD CHANGES ====== */
function telefonChange() {
    var raw = $('#txtkullanicitelefon').val();
    telefon = onlyDigits(raw);
    telefonDogrulandi = false;
    setVerifyUI('Phone', false);
}
function emailChange() {
    mailAdres = $('#txtkullanicimail').val().trim();
    mailAdresDogrulandi = false;
    setVerifyUI('Mail', false);
}

/* ====== SAVE / VERIFY ====== */
function changeUserInfos(action) {
    if (action === 'Mail') {
        var email = $('#txtkullanicimail').val().trim();
        if (!isValidEmail(email)) { if (window.toastr) toastr.error('Geçerli bir e-posta giriniz.'); return; }

        var $btn = $('#mailDogrulamagonder').prop('disabled', true);
        $.ajax({
            url: '/User/SendMailVerification',
            method: 'POST',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify({ email: email })
        })
            .done(function () { if (window.toastr) toastr.success('Doğrulama kodu e-postanıza gönderildi.'); })
            .fail(function (xhr) {
                if (window.toastr) toastr.error('Doğrulama kodu gönderilemedi.');
                console.error(xhr.responseText || xhr.statusText);
            })
            .always(function () { $btn.prop('disabled', false); });
        return;
    }

    if (action === 'Phone') {
        var digits = onlyDigits($('#txtkullanicitelefon').val());
        if (digits.length < 10) { if (window.toastr) toastr.error('Geçerli bir telefon giriniz.'); return; }

        var $btn2 = $('#telDogrulamagonder').prop('disabled', true);
        $.ajax({
            url: '/User/SendPhoneVerification',
            method: 'POST',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify({ phone: digits })
        })
            .done(function () { if (window.toastr) toastr.success('SMS doğrulama kodu gönderildi.'); })
            .fail(function (xhr) {
                if (window.toastr) toastr.error('SMS gönderilemedi.');
                console.error(xhr.responseText || xhr.statusText);
            })
            .always(function () { $btn2.prop('disabled', false); });
        return;
    }

    // Güncelle + (varsa) şifre değiştir
    if (!validatePasswordIfVisible()) return;

    var payload = {
        email: $('#txtkullanicimail').val().trim(),
        phone: onlyDigits($('#txtkullanicitelefon').val()),
        changePassword: $('#sifreDegistirmeAktif').is(':visible'),
        oldPassword: $('#txteskisifre').val(),
        newPassword: $('#txtyenisifre').val()
    };

    var $btnUpd = $('#btn_guncelle').prop('disabled', true);
    $.ajax({
        url: '/User/UpdateProfile',
        method: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(payload)
    })
        .done(function () {
            if (window.toastr) toastr.success('Bilgileriniz güncellendi.');
            mailAdres = payload.email; telefon = payload.phone;
        })
        .fail(function (xhr) {
            if (window.toastr) toastr.error('Güncelleme başarısız.');
            console.error(xhr.responseText || xhr.statusText);
        })
        .always(function () { $btnUpd.prop('disabled', false); });
}

/* ====== MESAJ / TALEP ====== */
function openMessageModal() {
    $('#message-modal').modal('show');
    loadMessages();
}
function loadMessages() {
    $('#messagesAll').html('<div class="text-muted">Yükleniyor...</div>');
    $.ajax({
        url: '/Support/ListTickets',
        method: 'GET',
        dataType: 'json'
    })
        .done(function (list) {
            if (!list || !list.length) {
                $('#messagesAll').html('<div class="alert alert-info">Kayıt bulunamadı.</div>');
                return;
            }
            var html = list.map(function (m) {
                return (
                    '<div class="callout callout-default" style="margin-bottom:12px">' +
                    '<b>' + escapeHtml(m.konu || '') + '</b>' +
                    '<div>' + escapeHtml(m.aciklama || '') + '</div>' +
                    '<small>' + escapeHtml(m.tarih || '') + ' • ' + escapeHtml(m.durum || '') + '</small>' +
                    '</div>'
                );
            }).join('');
            $('#messagesAll').html(html);
        })
        .fail(function (xhr) {
            $('#messagesAll').html('<div class="alert alert-warning">Mesajlar alınamadı.</div>');
            console.error(xhr.responseText || xhr.statusText);
        });
}
function sendTalep() {
    var data = {
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

    var $btn = $('#btnSendTalep').prop('disabled', true);

    $.ajax({
        url: '/Support/SendTicket',
        method: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(data)
    })
        .done(function () {
            if (window.toastr) toastr.success('İstek/Talep gönderildi.');
            $('#konutxt').val('');
            $('#aciklama').val('');
            loadMessages();
        })
        .fail(function (xhr) {
            if (window.toastr) toastr.error('İstek/Talep gönderilemedi.');
            console.error(xhr.responseText || xhr.statusText);
        })
        .always(function () { $btn.prop('disabled', false); });
}

/* ====== BEKLEYEN İNDİRMELER ====== */
function openDownloadExcelModal() {
    $('#downloadExcel-modal').modal('show');
    refreshDownloadTable();
}
function refreshDownloadTable() {
    $('#excelTable').html('<div class="text-muted">Yükleniyor...</div>');
    $.ajax({
        url: '/Downloads/Pending',
        method: 'GET',
        dataType: 'json'
    })
        .done(function (items) {
            if (!items || !items.length) {
                $('#excelTable').html('<div class="alert alert-info">Bekleyen indirmeniz yok.</div>');
                return;
            }
            var rows = items.map(function (it) {
                var link = it.url
                    ? '<a class="btn btn-xs btn-success" href="' + it.url + '" target="_blank">İndir</a>'
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
            var tableHtml =
                '<table class="table table-bordered">' +
                '<thead><tr><th>Dosya</th><th>Boyut</th><th>Durum</th><th>İşlem</th></tr></thead>' +
                '<tbody>' + rows + '</tbody>' +
                '</table>';
            $('#excelTable').html(tableHtml);
        })
        .fail(function (xhr) {
            $('#excelTable').html('<div class="alert alert-warning">Liste yüklenemedi.</div>');
            console.error(xhr.responseText || xhr.statusText);
        });
}

/* ====== INIT & EVENTS ====== */
$(function () {
    applyMasks();
    initSelect2();
    hydrateUserForm();

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
});
