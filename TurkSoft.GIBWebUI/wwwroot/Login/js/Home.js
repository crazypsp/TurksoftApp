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

    var id = raw.id || raw.Id || identifier || (idx + 1);

    return {
        id: id,
        Identifier: (identifier || '').toString().trim(),
        Title: (title || '').toString().trim(),
        Alias: (alias || '').toString().trim()
    };
}

// İlk yüklemede (term boş) liste iste
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
        var arr = Array.isArray(data) ? data : (data ? [data] : []);

        var slim = arr.map(normalizeMukellefRecord);
        window.MukellefList = slim;

        console.log('[Mükellef] İlk yüklenen kayıt sayısı (slim):', slim.length);
    } catch (err) {
        console.error('[Mükellef] SearchMukellef çağrısı sırasında hata:', err);
        window.MukellefList = [];
    }
}

// İstersen her input değişiminde sunucuya sor
async function searchMukellefOnServer(query) {
    try {
        var q = (query || '').trim();
        var url = '/Home/SearchMukellef?term=' + encodeURIComponent(q);

        var response = await fetch(url, {
            method: 'GET',
            headers: { 'Accept': 'application/json' }
        });

        if (!response.ok) {
            console.error('[Mükellef] SearchMukellef (query) başarısız. Status:', response.status, response.statusText);
            return [];
        }

        var data = await response.json();
        var arr = Array.isArray(data) ? data : (data ? [data] : []);
        return arr.map(normalizeMukellefRecord);
    } catch (err) {
        console.error('[Mükellef] SearchMukellef (query) hata:', err);
        return [];
    }
}

// ============================================
//  Fatura Oluştur modalı için Mükellef arama
//  (SearchMukellef ile datalist doldurur)
// ============================================

async function fillMusteriCariDatalistFromServer(filterText) {
    var dataList = document.getElementById('MusteriCariList');
    if (!dataList) {
        return;
    }

    var term = (filterText || '').toString().trim();
    console.log('[Mükellef] Datalist doldurma, term =', term);

    var results = await searchMukellefOnServer(term);

    // Eski seçenekleri temizle
    while (dataList.options.length > 0) {
        dataList.remove(0);
    }

    var maxItems = 50;
    results.slice(0, maxItems).forEach(function (m) {
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
 * Seçili mükellefi datalist'ten bulup sessionStorage'a yazar
 * ve yeni fatura sayfasına yönlendirir.
 */
function openCreateInvoiceWithSelectedMukellef(e) {
    if (e && e.preventDefault) e.preventDefault();

    var input = document.getElementById('checkInput');
    var dataList = document.getElementById('MusteriCariList');

    if (!input || !dataList) {
        alert('Mükellef arama alanı bulunamadı.');
        return;
    }

    var inputVal = (input.value || '').trim();
    if (!inputVal) {
        alert('Lütfen VKN/TCKN veya Ünvan girerek bir mükellef seçiniz.');
        return;
    }

    var selected = null;
    var options = dataList.options;
    for (var i = 0; i < options.length; i++) {
        if ((options[i].value || '').trim() === inputVal) {
            selected = options[i];
            break;
        }
    }

    if (!selected) {
        alert('Girilen değere uygun mükellef bulunamadı. Lütfen listeden birini seçiniz.');
        return;
    }

    var payload = {
        Identifier: selected.getAttribute('data-vkn') || '',
        Title: selected.getAttribute('data-title') || '',
        Alias: selected.getAttribute('data-alias') || ''
    };

    try {
        sessionStorage.setItem('SelectedMukellefForInvoice', JSON.stringify(payload));
        console.log('[Mükellef] SelectedMukellefForInvoice kaydedildi:', payload);
    } catch (err) {
        console.error('[Mükellef] SelectedMukellefForInvoice kaydedilemedi:', err);
    }

    window.location.href = '/EInvoice/CreateNewInvoice';
}

// Eski inline kullanım için global export (ihtiyaç olursa)
window.checkFatura = openCreateInvoiceWithSelectedMukellef;

/* ============================================
 *  TANIMLAMALAR MENÜSÜ (sadece Admin + Bayi)
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

    // 1 = Admin, 2 = Bayi, 3 = MM, 4 = Firma
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
        // Session yoksa burada hiçbir şeyi bozma; sayfada ne yazıyorsa o kalsın
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

    // Örnek: {id, gibFirmId, totalCredits, usedCredits, userId}
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

    // 0) İlk mükellef datası (isteğe bağlı preload)
    loadMukellefListFromJson();

    // =========================
    // 1) Session'dan Email & Firma oku
    // =========================
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

    // =========================
    // 2) RoleId – default 4 (Firma)
    // =========================
    var userRoleKey = 'UserRolId';
    var userRoleId = sessionStorage.getItem(userRoleKey) || sessionStorage.getItem('UserRoleId');

    if (!userRoleId) {
        userRoleId = '4';
        sessionStorage.setItem(userRoleKey, userRoleId);
        sessionStorage.setItem('UserRoleId', userRoleId);
        console.log('UserRol boş, 4 (Firma) olarak set edildi.');
    }

    // Menü görünürlüğü
    setupUserFirmMenuVisibility();

    // =========================
    // 3) GİB config + DOM
    // =========================
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

    // =========================
    // 4) Logolar
    // =========================
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

    // =========================
    // 5) Kredi bilgisi DOM'a bas
    // =========================
    applyCreditFromSession();
});

// ============================================
//  jQuery – global modallar + örnek arama
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

    // Fatura Oluştur modalındaki VKN / Ünvan arama alanı
    var $checkInput = $('#checkInput');
    if ($checkInput.length) {
        // İlk odaklandığında boş term ile listeyi doldur
        $checkInput.on('focus', function () {
            fillMusteriCariDatalistFromServer('');
        });

        // Yazdıkça SearchMukellef'e sorgu atan debounce
        var searchTimeout = null;
        $checkInput.on('input', function (e) {
            var val = e.target.value || '';
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(function () {
                fillMusteriCariDatalistFromServer(val);
            }, 250);
        });
    }

    // Fatura Oluştur butonu
    $('#checkFatura, #btnCreateInvoiceFromModal').on('click', openCreateInvoiceWithSelectedMukellef);

    // Buradan checkInput / arama işlemlerini server'a bağlayabilirsin:
    //
    // $(document).on('input', '#mukellefSearch', async function () {
    //     var q = $(this).val();
    //     var results = await searchMukellefOnServer(q);
    //     console.log('[Mükellef] Arama:', q, 'Sonuç adedi:', results.length);
    //     // TODO: results ile dropdown / listeyi doldur
    // });
});
