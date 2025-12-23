// ============================================
//  GLOBAL HELPERS
// ============================================

function inferNameFromEmail(mail) {
    if (!mail) return '';
    var local = (mail.split('@')[0] || '').trim();
    if (!local) return '';
    local = local.replace(/[._\-]+/g, ' ');
    return local.charAt(0).toUpperCase() + local.slice(1);
}

function setValueOrTextById(id, value) {
    if (!id) return;
    var el = document.getElementById(id);
    if (!el || value == null) return;

    if (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA' || el.tagName === 'SELECT') {
        el.value = value;
    } else {
        el.textContent = value;
    }
}

function parseTRInt(s) {
    if (!s) return 0;
    s = String(s)
        .replace(/\./g, '')
        .replace(',', '.')
        .replace(/[^\d.]/g, '');
    var n = parseFloat(s);
    return isNaN(n) ? 0 : Math.round(n);
}

function formatTRInt(n) {
    try {
        return Number(n).toLocaleString('tr-TR');
    } catch {
        return String(n);
    }
}

function injectVknIntoParagraphs(vkn) {
    if (!vkn) return;

    var target = document.querySelector('[data-vkn-target="true"]');
    if (!target) {
        var ps = document.querySelectorAll('p');
        for (var i = 0; i < ps.length; i++) {
            var txt = (ps[i].textContent || '').trim().toLowerCase();
            if (txt === 'vkn:' || txt === 'vkn' || txt.indexOf('vkn:') === 0) {
                target = ps[i];
                break;
            }
        }
    }

    if (!target) {
        console.warn('Vkn paragrafı bulunamadı, text ile eşleşen <p> yok.');
        return;
    }

    target.textContent = 'Vkn: ' + vkn;
}

// ====== EK: Name split helper (TCKN ise) ======

function _splitFullName(fullName) {
    var s = (fullName || '').toString().trim().replace(/\s+/g, ' ');
    if (!s) return { firstName: '', lastName: '' };

    var parts = s.split(' ');
    if (parts.length === 1) return { firstName: parts[0], lastName: '' };

    var lastName = parts[parts.length - 1];
    var firstName = parts.slice(0, parts.length - 1).join(' ');
    return { firstName: firstName, lastName: lastName };
}

function _uniqStrings(arr) {
    var out = [];
    var seen = Object.create(null);
    (arr || []).forEach(function (x) {
        var s = (x || '').toString().trim();
        if (!s) return;
        var key = s.toLowerCase();
        if (seen[key]) return;
        seen[key] = true;
        out.push(s);
    });
    return out;
}

/* ============================================
 *  MÜKELLEF LİSTESİ (HomeController üzerinden)
 * ==========================================*/

window.MukellefList = [];

function normalizeMukellefRecord(raw, idx) {
    if (!raw || typeof raw !== 'object') raw = {};

    var identifier =
        raw.Identifier ||
        raw.identifier ||
        raw.Vkn ||
        raw.vkn ||
        '';

    var title =
        raw.Title ||
        raw.title ||
        '';

    var alias =
        raw.Alias ||
        raw.alias ||
        raw.GibAlias ||
        raw.gibAlias ||
        '';

    // EK: Aliases array desteği (GetMukellefByIdentifier ile gelir)
    var aliases = raw.Aliases || raw.aliases || raw.AliasList || raw.aliasList || null;
    if (Array.isArray(aliases)) {
        aliases = _uniqStrings(aliases);
    } else {
        aliases = null;
    }

    var id = raw.id || raw.Id || identifier || (idx + 1);

    return {
        id: id,
        Identifier: (identifier || '').toString().trim(),
        Title: (title || '').toString().trim(),
        Alias: (alias || '').toString().trim(),
        Aliases: aliases
    };
}

// İlk yüklemede (term boş) liste iste (küçük preload)
async function loadMukellefListFromJson() {
    try {
        var url = '/Home/SearchMukellef?term=';
        console.log('[Mükellef] /Home/SearchMukellef istek başlıyor:', url);

        var response = await fetch(url, {
            method: 'GET',
            headers: { 'Accept': 'application/json' }
        });

        console.log('[Mükellef] /Home/SearchMukellef yanıtı:', response.status, response.statusText);

        if (!response.ok) {
            var txt = '';
            try { txt = await response.text(); } catch { }
            console.error('[Mükellef] SearchMukellef isteği başarısız. Status:', response.status, response.statusText, txt);
            window.MukellefList = [];
            return;
        }

        var data = await response.json();
        var arr = Array.isArray(data)
            ? data
            : (data && Array.isArray(data.items) ? data.items : []);

        var slim = arr.map(normalizeMukellefRecord);
        window.MukellefList = slim;

        console.log('[Mükellef] İlk yüklenen kayıt sayısı (slim):', slim.length);
    } catch (err) {
        console.error('[Mükellef] SearchMukellef çağrısı sırasında hata:', err);
        window.MukellefList = [];
    }
}

/**
 *  Sunucudan mükellef arama (isteğe bağlı sayfalama ile)
 */
async function searchMukellefOnServer(query, page, pageSize) {
    try {
        var q = (query || '').trim();
        var url = '/Home/SearchMukellef?term=' + encodeURIComponent(q);

        if (page != null && pageSize != null) {
            url += '&page=' + encodeURIComponent(page) +
                '&pageSize=' + encodeURIComponent(pageSize);
        }

        var response = await fetch(url, {
            method: 'GET',
            headers: { 'Accept': 'application/json' },
            cache: 'no-cache'
        });

        if (!response.ok) {
            console.error('[Mükellef] SearchMukellef (query) başarısız. Status:', response.status, response.statusText);
            return [];
        }

        var data = await response.json();
        var arr = Array.isArray(data)
            ? data
            : (data && Array.isArray(data.items) ? data.items : []);

        return arr.map(normalizeMukellefRecord);
    } catch (err) {
        console.error('[Mükellef] SearchMukellef (query) hata:', err);
        return [];
    }
}

// ============================================
//  Fatura Oluştur modalı için Mükellef arama
// ============================================

var _mukellefDatalistRequestId = 0;

async function fillMusteriCariDatalistFromServer(filterText) {
    var dataList = document.getElementById('MusteriCariList');
    if (!dataList) {
        return;
    }

    var term = (filterText || '').toString().trim();
    console.log('[Mükellef] Datalist doldurma, term =', term);

    if (term.length > 0 && term.length < 2) {
        console.log('[Mükellef] Arama için en az 2 karakter bekleniyor.');
        return;
    }

    var thisRequestId = ++_mukellefDatalistRequestId;

    var pageSize = 20;
    var page = 1;

    var results = await searchMukellefOnServer(term, page, pageSize);

    if (thisRequestId !== _mukellefDatalistRequestId) {
        return;
    }

    while (dataList.options.length > 0) {
        dataList.remove(0);
    }

    results.forEach(function (m) {
        var vkn = m.Identifier || '';
        var title = m.Title || '';
        var alias = m.Alias || '';

        var display = vkn && title
            ? (vkn + ' - ' + title)
            : (vkn || title);

        if (!display) return;

        var opt = document.createElement('option');
        opt.value = display;
        opt.setAttribute('data-id', m.id || '');
        opt.setAttribute('data-vkn', vkn);
        opt.setAttribute('data-title', title);
        opt.setAttribute('data-alias', alias);

        dataList.appendChild(opt);
    });

    console.log('[Mükellef] Datalist dolduruldu. Kayıt sayısı =', results.length);
}

/**
 * Girilen Identifier'a göre E-Fatura / E-Arşiv'e yönlendirme
 */
async function routeByIdentifier(identifierInput) {
    var vkn = (identifierInput || '').replace(/\D/g, '').slice(0, 11);
    if (vkn.length !== 10 && vkn.length !== 11) {
        alert('Lütfen 10 veya 11 haneli VKN/TCKN giriniz.');
        return;
    }

    try {
        // (performans için includeAliases kullanmıyoruz)
        var url = '/Home/GetMukellefByIdentifier?identifier=' + encodeURIComponent(vkn);
        console.log('[Mükellef] GetMukellefByIdentifier istek:', url);

        var resp = await fetch(url, {
            method: 'GET',
            headers: { 'Accept': 'application/json' },
            cache: 'no-cache'
        });

        if (!resp.ok) {
            console.error('[Mükellef] GetMukellefByIdentifier hata:', resp.status, resp.statusText);
            alert('Mükellef kontrolü sırasında bir hata oluştu.');
            return;
        }

        var data = await resp.json();
        console.log('[Mükellef] GetMukellefByIdentifier yanıt:', data);

        if (data && data.found) {
            var identifier = (data.Identifier || data.identifier || vkn || '').toString().trim();
            var title = (data.Title || data.title || '').toString().trim();
            var alias = (data.Alias || data.alias || '').toString().trim();

            var payload = { Identifier: identifier, Title: title, Alias: alias };

            try {
                sessionStorage.setItem('SelectedMukellefForInvoice', JSON.stringify(payload));
            } catch (e) {
                console.error('SelectedMukellefForInvoice yazılamadı:', e);
            }

            console.log('[Mükellef] E-Fatura mükellefi bulundu, /EInvoice/CreateNewInvoice yönleniyor...');
            window.location.href = '/EInvoice/CreateNewInvoice';
            return;
        }

        var eaPayload = { Identifier: vkn };
        try {
            sessionStorage.setItem('SelectedMukellefForEArchive', JSON.stringify(eaPayload));
        } catch (e) {
            console.error('SelectedMukellefForEArchive yazılamadı:', e);
        }

        console.log('[Mükellef] Mükellef listede yok, /EArchive/CreateNewEarchiveInvoice yönleniyor...');
        window.location.href = '/EArchive/CreateNewEarchiveInvoice';
    } catch (err) {
        console.error('[Mükellef] routeByIdentifier exception:', err);
        alert('Mükellef kontrolü sırasında bir hata oluştu.');
    }
}

async function openCreateInvoiceWithSelectedMukellef(e) {
    if (e && e.preventDefault) e.preventDefault();

    var input = document.getElementById('checkInput');
    if (!input) {
        alert('VKN/TCKN alanı bulunamadı.');
        return;
    }

    var digits = (input.value || '').replace(/\D/g, '').slice(0, 11);
    input.value = digits;

    if (digits.length !== 10 && digits.length !== 11) {
        alert('Lütfen 10 veya 11 haneli VKN/TCKN giriniz.');
        return;
    }

    await routeByIdentifier(digits);
}

window.checkFatura = openCreateInvoiceWithSelectedMukellef;

/* ============================================
 *  TANIMLAMALAR MENÜSÜ
 * ==========================================*/

function setupUserFirmMenuVisibility() {
    var menuEl = document.getElementById('userFirmMenu');
    if (!menuEl) {
        return;
    }

    var roleId = null;
    try {
        roleId = sessionStorage.getItem('UserRolId') || sessionStorage.getItem('UserRoleId');
    } catch (e) {
        console.warn('[Sidebar] UserRolId okunamadı:', e);
    }

    var showForAdminOrBayi = (roleId === '1' || roleId === '2');

    if (showForAdminOrBayi) {
        menuEl.style.display = '';
    } else {
        menuEl.style.display = 'none';
    }

    console.log('[Sidebar] Tanımlamalar menüsü kontrolü. roleId =', roleId, 'görünür mü?', showForAdminOrBayi);
}

/* ============================================
 *  KREDİ (KONTÖR) BİLGİSİ
 * ==========================================*/

function applyCreditFromSession() {
    var creditTotalEl = document.getElementById('creditTotal');
    var creditRemainderEl = document.getElementById('creditRemainder');
    var creditStatusEl = document.getElementById('creditStatus');
    var kampanyaDiv = document.getElementById('kampanyaStatus');
    var kampanyaTextEl = document.getElementById('kampanyaStatusText');

    if (!creditTotalEl || !creditRemainderEl) {
        return;
    }

    var creditRaw = null;
    try {
        creditRaw = sessionStorage.getItem('UserCreditAccount');
    } catch (e) {
        console.warn('[Credit] UserCreditAccount session okunamadı:', e);
    }

    if (!creditRaw) {
        console.log('[Credit] Session\'da UserCreditAccount yok, mevcut HTML değerleri korunuyor.');
        return;
    }

    var credit;
    try {
        credit = JSON.parse(creditRaw);
    } catch (e) {
        console.error('[Credit] UserCreditAccount JSON parse hatası:', e, creditRaw);
        return;
    }

    console.log('[Credit] Ham kredi objesi:', credit);

    function pickNumber(obj, names, fallback) {
        if (!obj) return fallback;
        for (var i = 0; i < names.length; i++) {
            var v = obj[names[i]];
            if (v == null) continue;
            var n = Number(v);
            if (!isNaN(n)) return n;
        }
        return fallback;
    }

    var total = pickNumber(credit, [
        'totalCredits', 'TotalCredits',
        'LoadedCredit', 'loadedCredit',
        'TotalCredit', 'totalCredit',
        'Total', 'total',
        'YuklenenKontor', 'yuklenenKontor'
    ], null);

    var spent = pickNumber(credit, [
        'usedCredits', 'UsedCredits',
        'UsedCredit', 'usedCredit',
        'Spent', 'spent',
        'Used', 'used',
        'HarcananKontor', 'harcananKontor'
    ], null);

    var remainder = pickNumber(credit, [
        'RemainingCredit', 'remainingCredit',
        'Remainder', 'remainder',
        'Remaining', 'remaining',
        'KalanKontor', 'kalanKontor'
    ], null);

    if (remainder == null && total != null && spent != null) {
        remainder = Math.max(0, total - spent);
    }
    if (spent == null && total != null && remainder != null) {
        spent = Math.max(0, total - remainder);
    }

    if (total != null && isFinite(total)) {
        creditTotalEl.textContent = formatTRInt(total);
    }
    if (remainder != null && isFinite(remainder)) {
        creditRemainderEl.textContent = formatTRInt(remainder);
    }
    if (creditStatusEl && spent != null && isFinite(spent)) {
        creditStatusEl.textContent = formatTRInt(spent);
    }

    if (kampanyaDiv && kampanyaTextEl) {
        var kampText =
            credit.PromotionStatus ??
            credit.promotionStatus ??
            credit.CampaignStatus ??
            credit.campaignStatus ??
            credit.KampanyaStatus ??
            credit.kampanyaStatus ??
            credit.KampanyaDurumu ??
            credit.kampanyaDurumu ??
            '';

        if (kampText) {
            kampanyaTextEl.textContent = String(kampText);
            kampanyaDiv.hidden = false;
        }
    }

    console.log('[Credit] Kontör bilgisi DOM\'a uygulandı:', {
        total: total,
        remainder: remainder,
        spent: spent
    });
}

// ============================================
//  DOMContentLoaded
// ============================================

document.addEventListener('DOMContentLoaded', function () {

    loadMukellefListFromJson();

    var email = (sessionStorage.getItem('lastLoginEmail') || '').trim().toLowerCase();
    console.log('lastLoginEmail:', email);

    var firma = null;
    var firmaRaw = sessionStorage.getItem('Firma');
    if (firmaRaw) {
        try {
            firma = JSON.parse(firmaRaw);
            console.log('Firma (sessionStorage):', firma);
        } catch (e) {
            console.error('Firma JSON parse hatası:', e, firmaRaw);
            firma = null;
        }
    }

    var userRoleKey = 'UserRolId';
    var userRoleId = sessionStorage.getItem(userRoleKey) || sessionStorage.getItem('UserRoleId');

    if (!userRoleId) {
        userRoleId = '4';
        sessionStorage.setItem(userRoleKey, userRoleId);
        sessionStorage.setItem('UserRoleId', userRoleId);
        console.log('UserRol boş, 4 (Firma) olarak set edildi.');
    }

    setupUserFirmMenuVisibility();

    if (firma) {
        var apiKey = firma.ApiKey || firma.apiKey || '';
        var senderVkn = firma.TaxNo || firma.taxNo || '';
        var inboxAlias = firma.GibAlias || firma.gibAlias || '';

        window.GibConfig = window.GibConfig || {};

        if (apiKey) window.GibConfig.ApiKey = apiKey;
        if (senderVkn) window.GibConfig.TestSenderVkn = senderVkn;
        if (inboxAlias) window.GibConfig.TestInboxAlias = inboxAlias;

        console.log('GibConfig:', window.GibConfig);

        setValueOrTextById('TaxNo', senderVkn);
        setValueOrTextById('FirmaTaxNo', senderVkn);
        setValueOrTextById('Vkn', senderVkn);

        setValueOrTextById('GibAlias', inboxAlias);
        setValueOrTextById('FirmaGibAlias', inboxAlias);

        setValueOrTextById('ApiKey', apiKey);
        setValueOrTextById('FirmaApiKey', apiKey);

        injectVknIntoParagraphs(senderVkn);
    }

    var logo = document.getElementById('userLogo');
    var mainOne = document.getElementById('mainOne');
    var maintwo = document.getElementById('maintwo');
    var maintree = document.getElementById('maintree');
    var mainfour = document.getElementById('mainfour');

    var basePath = '/login/images/';

    var map = {
        'firma@gib.com': 'Nox Müsavirim Portal-06.png',
        'mm@gib.com': 'Nox Müsavirim Portal-05.png',
        'bayi@gib.com': 'Nox Müsavirim Portal-09.png',
        'fatura@gib.com': 'Nox Müsavirim Portal-09.png'
    };

    var mapOne = {
        'firma@gib.com': 'Nox Müsavirim Portal-01.png',
        'bayi@gib.com': 'Nox Müsavirim Portal-10.png',
        'fatura@gib.com': 'Nox Müsavirim Portal-10.png'
    };

    var fileName = map[email] || 'Nox Müsavirim Portal-06.png';
    var fileNameOne = mapOne[email] || 'Nox Müsavirim Portal-01.png';

    if (logo) {
        logo.src = basePath + fileName;
        logo.style.display = 'block';
    }
    if (mainOne) {
        mainOne.src = basePath + fileNameOne;
        mainOne.style.display = 'block';
    }
    if (maintwo) {
        maintwo.src = basePath + fileNameOne;
        maintwo.style.display = 'block';
    }
    if (maintree) {
        maintree.src = basePath + fileName;
        maintree.style.display = 'block';
    }
    if (mainfour) {
        mainfour.src = basePath + fileName;
        mainfour.style.display = 'block';
    }

    var sirketSpan = document.getElementById('sirketname');
    if (sirketSpan) {
        var nameMap = {
            'firma@gib.com': 'Firma',
            'mm@gib.com': 'MM',
            'bayi@gib.com': 'Bayi',
            'fatura@gib.com': 'Faturam Türk'
        };

        var displayName = null;

        if (firma && (firma.Title || firma.title)) {
            displayName = firma.Title || firma.title;
        } else {
            displayName = nameMap[email] || inferNameFromEmail(email);
        }

        if (displayName) {
            sirketSpan.textContent = displayName;
        }
    }

    applyCreditFromSession();
});

// ============================================
//  jQuery – global modallar
// ============================================

$(function () {
    $('#createInvoice').on('click', function (e) {
        e.preventDefault();
        $('#faturaOlustur').modal('show');
    });

    $('#envelopeCheckModalCol').on('click', function (e) {
        e.preventDefault();
        $('#modal-envelope').modal('show');
    });

    $('[data-toggle="tooltip"]').tooltip();

    var $checkInput = $('#checkInput');
    if ($checkInput.length) {
        $checkInput.on('input', function (e) {
            var val = (e.target.value || '').replace(/\D/g, '');
            if (val.length > 11) {
                val = val.slice(0, 11);
            }
            e.target.value = val;

            if (val.length === 10 || val.length === 11) {
                if (e.target.dataset.lastChecked === val) {
                    return;
                }
                e.target.dataset.lastChecked = val;
                routeByIdentifier(val);
            } else {
                e.target.dataset.lastChecked = '';
            }
        });
    }

    $('#checkFatura, #btnCreateInvoiceFromModal').on('click', openCreateInvoiceWithSelectedMukellef);
});

// ============================================
//  CreateNewInvoice – Alıcı Select2 (EK KOD)
// ============================================

// ====== EK: Alias select'i komple rebuild eden fonksiyon ======
function _applyAliasesToAliciEtiketi(aliasSelectId, aliases, selectedAlias) {
    var sel = document.getElementById(aliasSelectId);
    if (!sel) return;

    // Default HTML option dahil HEPSİNİ SİL (madde 2)
    while (sel.options.length > 0) {
        sel.remove(0);
    }

    var list = _uniqStrings(aliases || []);
    if (!list.length) {
        // Boş kalmasın diye tek empty option bırak
        sel.appendChild(new Option('', '', true, true));
    } else {
        list.forEach(function (a, idx) {
            var opt = new Option(a, a, false, false);
            sel.appendChild(opt);
        });

        // seçilecek alias
        var pick = (selectedAlias || '').toString().trim();
        if (pick) {
            var found = false;
            for (var i = 0; i < sel.options.length; i++) {
                if (sel.options[i].value === pick) {
                    sel.selectedIndex = i;
                    found = true;
                    break;
                }
            }
            if (!found) {
                sel.selectedIndex = 0;
            }
        } else {
            sel.selectedIndex = 0;
        }
    }

    if (typeof $ !== 'undefined' && $.fn && $.fn.select2 && $(sel).hasClass('select2-hidden-accessible')) {
        $(sel).trigger('change');
    } else {
        // normal select change
        try { sel.dispatchEvent(new Event('change', { bubbles: true })); } catch { }
    }
}

// ====== EK: Detay çekme (Alias listesi için) ======
var _getMukellefDetailsReqId = 0;

async function _getMukellefDetailsByIdentifier(identifier) {
    var vkn = (identifier || '').toString().replace(/\D/g, '').slice(0, 11);
    if (vkn.length !== 10 && vkn.length !== 11) return { found: false };

    var myId = ++_getMukellefDetailsReqId;

    var url = '/Home/GetMukellefByIdentifier?identifier=' + encodeURIComponent(vkn) + '&includeAliases=true';
    try {
        var resp = await fetch(url, {
            method: 'GET',
            headers: { 'Accept': 'application/json' },
            cache: 'no-cache'
        });

        if (myId !== _getMukellefDetailsReqId) return { found: false, cancelled: true };

        if (!resp.ok) {
            console.error('[CreateInvoice] GetMukellefByIdentifier(detail) hata:', resp.status, resp.statusText);
            return { found: false };
        }

        var data = await resp.json();
        return data || { found: false };
    } catch (e) {
        console.error('[CreateInvoice] GetMukellefByIdentifier(detail) exception:', e);
        return { found: false };
    }
}

/**
 * Seçilen mükellefi CreateNewInvoice.cshtml içindeki alıcı alanlarına uygular.
 */
function _fillAliciFromMukellefForCreateInvoice(m) {
    var vknSearchId = 'AliciVknTckn';
    var vknTargetId = 'txtIdentificationID';
    var aliasSelectId = 'ddlAliciEtiketi';
    var partyNameId = 'txtPartyName';
    var personFirstId = 'txtPersonFirstName';
    var personLastId = 'txtPersonLastName';

    if (!m) {
        setValueOrTextById(vknSearchId, '');
        setValueOrTextById(vknTargetId, '');
        setValueOrTextById(partyNameId, '');
        setValueOrTextById(personFirstId, '');
        setValueOrTextById(personLastId, '');

        _applyAliasesToAliciEtiketi(aliasSelectId, [], '');
        return;
    }

    var identifier = (m.Identifier || m.identifier || '').toString().trim();
    var title = (m.Title || m.title || '').toString().trim();
    var alias = (m.Alias || m.alias || '').toString().trim();

    // Alias listesi (madde 2)
    var aliases = m.Aliases || m.aliases || null;
    if (Array.isArray(aliases)) {
        aliases = _uniqStrings(aliases);
    } else {
        aliases = alias ? [alias] : [];
    }

    setValueOrTextById(vknSearchId, identifier);
    setValueOrTextById(vknTargetId, identifier);

    setValueOrTextById(partyNameId, title);

    // (madde 3) TCKN ise kişi adı/soyadı anlamlı doldur
    var digits = identifier.replace(/\D/g, '');
    if (digits.length === 11) {
        var split = _splitFullName(title);
        setValueOrTextById(personFirstId, split.firstName);
        setValueOrTextById(personLastId, split.lastName);
    } else {
        setValueOrTextById(personFirstId, '');
        setValueOrTextById(personLastId, '');
    }

    _applyAliasesToAliciEtiketi(aliasSelectId, aliases, alias);
}

/**
 * CreateNewInvoice.cshtml'de yer alan ddlMusteriAra select'i için Select2 + server-side mükellef arama.
 */
function initCreateNewInvoiceAliciSelect2() {
    if (typeof $ === 'undefined' || !$.fn || !$.fn.select2) {
        console.warn('[CreateInvoice] jQuery veya Select2 yüklü değil, initCreateNewInvoiceAliciSelect2 atlandı.');
        return;
    }

    var $ddl = $('#ddlMusteriAra');
    if (!$ddl.length) {
        return;
    }

    try {
        if ($ddl.hasClass('select2-hidden-accessible')) {
            $ddl.select2('destroy');
        }
    } catch (e) {
        console.warn('[CreateInvoice] Mevcut Select2 destroy edilemedi:', e);
    }

    var pageSize = 20;

    $ddl.select2({
        placeholder: $ddl.data('placeholder') || 'Mükellef seçiniz',
        allowClear: true,
        minimumInputLength: 2,
        language: {
            inputTooShort: function (args) {
                var remainingChars = args.minimum - args.input.length;
                return 'Lütfen en az ' + remainingChars + ' karakter daha yazın';
            },
            noResults: function () {
                return 'Sonuç bulunamadı';
            },
            searching: function () {
                return 'Aranıyor...';
            }
        },
        ajax: {
            url: '/Home/SearchMukellef',
            dataType: 'json',
            delay: 250,
            cache: true,
            data: function (params) {
                var page = params.page || 1;
                return {
                    term: params.term || '',
                    page: page,
                    pageSize: pageSize
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 1;

                var arr = Array.isArray(data)
                    ? data
                    : (data && Array.isArray(data.items) ? data.items : []);

                // EK: Client-side dedupe (madde 1’i garantiye almak için)
                var seen = Object.create(null);
                var results = [];

                arr.forEach(function (raw, idx) {
                    var m = (typeof normalizeMukellefRecord === 'function')
                        ? normalizeMukellefRecord(raw, idx)
                        : (raw || {});

                    var key = (m.Identifier || '').toString().trim();
                    if (!key) return;

                    if (seen[key]) return;
                    seen[key] = true;

                    var text = (m.Identifier && m.Title)
                        ? (m.Identifier + ' - ' + m.Title)
                        : (m.Identifier || m.Title || '');

                    results.push({
                        id: m.Identifier, // EK: stabil id
                        text: text,
                        mukellef: m
                    });
                });

                var total = (data && (data.total || data.Total)) || (params.page * pageSize + 1);
                var more = (params.page * pageSize) < total;

                return {
                    results: results,
                    pagination: { more: more }
                };
            }
        }
    });

    // Seçim yapılınca: detay çek (alias listesi) ve doldur
    $ddl.on('select2:select', async function (e) {
        var data = e.params.data;
        if (!data || !data.mukellef) return;

        var picked = data.mukellef;
        var idDigits = (picked.Identifier || '').toString();

        // Önce hızlı doldur
        _fillAliciFromMukellefForCreateInvoice(picked);

        // Sonra detay (tüm alias) çek ve yeniden uygula
        var detail = await _getMukellefDetailsByIdentifier(idDigits);
        if (detail && detail.found) {
            var merged = normalizeMukellefRecord(detail, 0);
            // normalize; Identifier/Title/Alias/Aliases gelir
            _fillAliciFromMukellefForCreateInvoice(merged);
        }
    });

    $ddl.on('select2:clear', function () {
        _fillAliciFromMukellefForCreateInvoice(null);
    });

    console.log('[CreateInvoice] ddlMusteriAra Select2 + server-side mükellef arama hazır.');
}

// ============================================
//  Alıcı VKN/TCKN inputundan (AliciVknTckn) arama
// ============================================

async function _searchAndFillAliciByVknFromInput() {
    var input = document.getElementById('AliciVknTckn');
    if (!input) return;

    var raw = (input.value || '').toString().trim();
    var digits = raw.replace(/\D/g, '').slice(0, 11);

    input.value = digits;

    if (digits.length !== 10 && digits.length !== 11) {
        return;
    }

    try {
        // EK: direkt detay çek (alias listesi için)
        var detail = await _getMukellefDetailsByIdentifier(digits);

        if (detail && detail.found) {
            var m = normalizeMukellefRecord(detail, 0);
            _fillAliciFromMukellefForCreateInvoice(m);

            // ddlMusteriAra Select2 varsa set edelim
            if (typeof $ !== 'undefined' && $.fn && $.fn.select2) {
                var $ddl = $('#ddlMusteriAra');
                if ($ddl.length) {
                    var text = (m.Identifier && m.Title)
                        ? (m.Identifier + ' - ' + m.Title)
                        : (m.Identifier || m.Title || '');

                    var optVal = m.Identifier;
                    var $existing = $ddl.find('option[value="' + optVal + '"]');

                    if (!$existing.length) {
                        var newOption = new Option(text, optVal, true, true);
                        $ddl.append(newOption);
                    }

                    $ddl.val(optVal).trigger('change');
                }
            }
        } else {
            _fillAliciFromMukellefForCreateInvoice({
                Identifier: digits,
                Title: '',
                Alias: '',
                Aliases: []
            });
        }
    } catch (err) {
        console.error('[CreateInvoice] _searchAndFillAliciByVknFromInput hata:', err);
    }
}

function initAliciVknTcknSearchBinding() {
    var input = document.getElementById('AliciVknTckn');
    if (!input) return;

    input.addEventListener('input', function (e) {
        var val = (e.target.value || '').replace(/\D/g, '');
        if (val.length > 11) {
            val = val.slice(0, 11);
        }
        e.target.value = val;

        if (val.length === 10 || val.length === 11) {
            if (e.target.dataset.lastChecked === val) {
                return;
            }
            e.target.dataset.lastChecked = val;
            _searchAndFillAliciByVknFromInput();
        } else {
            e.target.dataset.lastChecked = '';
        }
    });
}

document.addEventListener('DOMContentLoaded', function () {
    try {
        initCreateNewInvoiceAliciSelect2();
        initAliciVknTcknSearchBinding();
    } catch (e) {
        console.error('[CreateInvoice] CreateInvoice init hata:', e);
    }
});
