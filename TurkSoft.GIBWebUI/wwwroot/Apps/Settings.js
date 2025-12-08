// wwwroot/apps/settings.js
import {
    DbConnectionSettingApi,
    InvoiceDesignTemplateApi,
    EmailSettingApi,
    InvoiceNumberSettingApi,
    NotificationSettingApi,
    UserInvoiceInboxSettingApi,
    ParameterSettingApi,
    UserVerificationDeviceApi,
    UserApi,
    RoleApi,
    UserRolesApi,
    SettingApi
} from '../Entites/index.js';

/* ============================================================
   Yardımcılar: RowVersion, BaseEntity, UserId, Toastr
   ============================================================ */

const DEFAULT_ROWVERSION_HEX = '0x00000000000007E1';
const DEFAULT_ROWVERSION_BASE64 = rowVersionHexToBase64(DEFAULT_ROWVERSION_HEX);

// Bu modül içinde global current user id
let SETTINGS_CURRENT_USER_ID = 0;

// Bazı listeler cache için
let _dbConnectionSetting = null;
let _emailSetting = null;
const _invoiceNumberMap = {};
let _deviceList = [];
let _usersCache = [];
let _rolesCache = [];
let _userInboxList = [];
let _parameterList = [];

/**
 * 0x... RowVersion → base64
 */
function rowVersionHexToBase64(hex) {
    if (!hex) return '';
    let h = String(hex).trim();
    if (h.startsWith('0x') || h.startsWith('0X')) {
        h = h.substring(2);
    }
    if (h.length % 2 === 1) {
        h = '0' + h;
    }
    const bytes = [];
    for (let i = 0; i < h.length; i += 2) {
        const b = parseInt(h.substr(i, 2), 16);
        if (!isNaN(b)) bytes.push(b);
    }
    let bin = '';
    for (let i = 0; i < bytes.length; i++) {
        bin += String.fromCharCode(bytes[i]);
    }
    try {
        return btoa(bin);
    } catch (e) {
        console.error('[Settings] RowVersion base64 dönüşümünde hata:', e);
        return '';
    }
}

function getRowVersionB64FromEntity(entity) {
    if (!entity) return '';
    let rv =
        entity.rowVersionBase64 || entity.RowVersionBase64 ||
        entity.rowVersion || entity.RowVersion ||
        entity.rowVersionHex || entity.RowVersionHex;

    if (rv && /^0x/i.test(rv)) {
        rv = rowVersionHexToBase64(rv);
    }
    return rv || '';
}

/**
 * Mevcut entity varsa Created* ve IsActive gibi alanları korur,
 * yoksa yeni kayıt için default değerler üretir.
 */
function buildBaseEntityForSave(userId, existingEntity) {
    const nowIso = new Date().toISOString();
    userId = userId || 0;

    if (existingEntity) {
        const createdAt = existingEntity.createdAt || existingEntity.CreatedAt || nowIso;
        const isActive = (existingEntity.isActive !== undefined
            ? existingEntity.isActive
            : (existingEntity.IsActive !== undefined ? existingEntity.IsActive : true));

        const deleteDate = existingEntity.deleteDate || existingEntity.DeleteDate || null;
        const deletedByUserId = existingEntity.deletedByUserId || existingEntity.DeletedByUserId || null;
        const createdByUserId = existingEntity.createdByUserId || existingEntity.CreatedByUserId || userId;

        const rvB64 = getRowVersionB64FromEntity(existingEntity) || DEFAULT_ROWVERSION_BASE64;

        return {
            UserId: existingEntity.userId || existingEntity.UserId || userId,
            IsActive: isActive,
            DeleteDate: deleteDate,
            DeletedByUserId: deletedByUserId,
            CreatedAt: createdAt,
            UpdatedAt: nowIso,
            CreatedByUserId: createdByUserId,
            UpdatedByUserId: userId,
            RowVersion: rvB64
        };
    }

    // Yeni kayıt
    return {
        UserId: userId,
        IsActive: true,
        DeleteDate: null,
        DeletedByUserId: null,
        CreatedAt: nowIso,
        UpdatedAt: nowIso,
        CreatedByUserId: userId,
        UpdatedByUserId: userId,
        RowVersion: DEFAULT_ROWVERSION_BASE64
    };
}

/**
 * GİB için kullanılacak userId'yi UI'dan / global'den okur
 * (User.js ile aynı mantık)
 */
function getCurrentUserIdForGib() {
    let userId = 0;

    // 1) Hidden input
    const $userHidden = $('#hdnUserId');
    if ($userHidden.length) {
        const parsed = parseInt($userHidden.val(), 10);
        if (!isNaN(parsed) && parsed > 0) {
            userId = parsed;
        }
    }

    // 2) window.currentUserId
    if (!userId && typeof window.currentUserId === 'number' && window.currentUserId > 0) {
        userId = window.currentUserId;
    }

    // 3) sessionStorage
    if (!userId && typeof sessionStorage !== 'undefined') {
        try {
            const stored =
                sessionStorage.getItem('CurrentUserId') ||
                sessionStorage.getItem('currentUserId') ||
                sessionStorage.getItem('UserId');

            if (stored) {
                const parsed = parseInt(stored, 10);
                if (!isNaN(parsed) && parsed > 0) {
                    userId = parsed;
                    console.log('[Settings] userId sessionStorage\'dan okundu:', userId);
                }
            }
        } catch (e) {
            console.warn('[Settings] CurrentUserId sessionStorage\'dan okunamadı:', e);
        }
    }

    if (!userId) {
        throw new Error('Kullanıcı Id (userId) bulunamadı. Lütfen oturumu kontrol edin.');
    }

    return userId;
}

function showSuccess(msg) {
    if (window.toastr) {
        window.toastr.success(msg);
    } else {
        alert(msg);
    }
}

function showError(err) {
    console.error('[Settings] Hata:', err);
    const msg = (err && (err.message || err.Message)) || 'İşlem sırasında hata oluştu.';
    if (window.toastr) {
        window.toastr.error(msg);
    } else {
        alert(msg);
    }
}

/**
 * Hem PascalCase hem camelCase'e toleranslı property getter
 */
function prop(obj, pascalName) {
    if (!obj) return undefined;
    const camel = pascalName.charAt(0).toLowerCase() + pascalName.slice(1);
    if (obj[pascalName] !== undefined) return obj[pascalName];
    return obj[camel];
}

/* ============================================================
   SAYFA INIT
   ============================================================ */

$(document).ready(function () {
    try {
        SETTINGS_CURRENT_USER_ID = getCurrentUserIdForGib();
    } catch (e) {
        console.warn('[Settings] Kullanıcı Id okunamadı, 0 kullanılacak.', e);
        SETTINGS_CURRENT_USER_ID = 0;
    }

    initSettingsPage();
});

async function initSettingsPage() {
    try {
        await Promise.all([
            initDbSettingsTab(),
            initXsltTab(),
            initUsersRolesTab(),
            initEmailSettingsTab(),
            initInvoiceNumberTab(),
            initNotificationTab(),
            initUserInvoiceInboxTab(),
            initParameterTab(),
            initDeviceTab(),
            initSpecialSettingsTab()
        ]);
    } catch (err) {
        showError(err);
    }
}

/* ============================================================
   1) VERİ TABANI AYARLARI — DbConnectionSetting
   ============================================================ */

async function initDbSettingsTab() {
    if (!$('#VeritabaniAyarlari').length) return;

    await loadDbConnectionSetting();

    $('#veriTabaniKaydet').off('click').on('click', async function () {
        try {
            await saveDbConnectionSetting();
        } catch (err) {
            showError(err);
        }
    });

    $('#veriTabaniTest').off('click').on('click', function () {
        // Burayı backend'deki test endpoint'ine göre uyarlayabilirsin
        alert('Veri tabanı bağlantı testi için backend endpoint\'i ile entegrasyon yapılmalıdır.');
    });

    // Luca butonu stub
    window.LucaModal = function () {
        alert('Luca ayar ekranı henüz entegrasyonlu değil.');
    };
}

async function loadDbConnectionSetting() {
    if (!DbConnectionSettingApi || !DbConnectionSettingApi.list) return;

    try {
        const list = await DbConnectionSettingApi.list();
        _dbConnectionSetting = Array.isArray(list) && list.length ? list[0] : null;
    } catch (err) {
        console.warn('[Settings] DbConnectionSetting list alınamadı:', err);
        _dbConnectionSetting = null;
        return;
    }

    if (!_dbConnectionSetting) {
        $('#txtServerAdi').val('');
        $('#txtPort').val('');
        $('#txtDbKullanici').val('');
        $('#txtDbSifre').val('');
        $('#txtVeritabaniAdi').val('');
        $('#txtDbase').val('');
        $('#txtFirebird').val('');
        $('#ddlveritabanitipi').val('1');
        return;
    }

    const s = _dbConnectionSetting;

    $('#txtServerAdi').val(prop(s, 'Server') || '');
    $('#txtPort').val(prop(s, 'Port') || '');
    $('#txtDbKullanici').val(prop(s, 'UserName') || '');
    $('#txtVeritabaniAdi').val(prop(s, 'Database') || '');
    $('#txtDbase').val(prop(s, 'DbasePath') || '');
    $('#txtFirebird').val(prop(s, 'FirebirdPath') || '');
    $('#EntegratorUsername').val(prop(s, 'EntegratorUserName') || '');

    const provider = (prop(s, 'Provider') || '').toString().toLowerCase();
    let ddlProvider = '1';
    if (provider.includes('firebird')) ddlProvider = '2';
    else if (provider.includes('oracle')) ddlProvider = '3';
    else if (provider.includes('odbc')) ddlProvider = '4';
    else if (provider.includes('oledb')) ddlProvider = '5';
    else if (provider.includes('mysql')) ddlProvider = '8';
    else if (provider.includes('db2')) ddlProvider = '9';
    $('#ddlveritabanitipi').val(ddlProvider);
}

async function saveDbConnectionSetting() {
    if (!DbConnectionSettingApi || !DbConnectionSettingApi.create) {
        throw new Error('DbConnectionSettingApi tanımlı değil.');
    }

    const server = ($('#txtServerAdi').val() || '').trim();
    const databaseName = ($('#txtVeritabaniAdi').val() || '').trim();

    if (!server) {
        alert('Server Adı zorunludur.');
        return;
    }
    if (!databaseName) {
        alert('Veritabanı Adı zorunludur.');
        return;
    }

    const portStr = ($('#txtPort').val() || '').trim();
    const port = portStr ? Number(portStr) : null;

    const vbAyartipiText = $('#ddlveritabaniayartipi option:selected').text() || 'Varsayılan';

    const providerVal = $('#ddlveritabanitipi').val();
    let providerName = 'SqlServer';
    switch (providerVal) {
        case '2': providerName = 'Firebird'; break;
        case '3': providerName = 'Oracle'; break;
        case '4': providerName = 'Odbc'; break;
        case '5': providerName = 'OleDb'; break;
        case '8': providerName = 'MySql'; break;
        case '9': providerName = 'Db2'; break;
        default: providerName = 'SqlServer'; break;
    }

    const existing = _dbConnectionSetting;
    const base = buildBaseEntityForSave(SETTINGS_CURRENT_USER_ID, existing);

    // DbConnectionSetting entity'sine göre sade DTO
    const dto = {
        ...base,
        Id: existing ? (existing.Id || existing.id) : 0,
        Name: vbAyartipiText,
        Provider: providerName,
        Server: server,
        Database: databaseName,
        Port: port,
        UserName: ($('#txtDbKullanici').val() || '').trim(),
        Password: ($('#txtDbSifre').val() || '').trim(),
        IntegratedSecurity: false,
        UseSsl: false,
        IsDefault: true
    };

    let saved;
    if (existing && (existing.Id || existing.id)) {
        const id = existing.Id || existing.id;
        saved = await DbConnectionSettingApi.update(id, dto);
        showSuccess('Veri tabanı ayarları güncellendi.');
    } else {
        saved = await DbConnectionSettingApi.create(dto);
        showSuccess('Veri tabanı ayarları kaydedildi.');
    }

    _dbConnectionSetting = saved;
}

/* ============================================================
   2) XSLT TASARIM — InvoiceDesignTemplate (basit iskelet)
   ============================================================ */

async function initXsltTab() {
    if (!$('#XsltTasarim').length) return;

    $('#btnXsltKaydet').off('click').on('click', async function () {
        try {
            await saveXsltTemplate();
        } catch (err) {
            showError(err);
        }
    });

    $('#btnXsltOnizle').off('click').on('click', function () {
        alert('Önizleme özelliği için backend’de XSLT önizleme endpoint’i ile entegrasyon yapılmalıdır.');
    });

    $('#btnYeniXslt').off('click').on('click', function () {
        $('#XsltName').val('');
        $('#colorinput').val('#000000');
        $('#FileFirmaLogo').val('');
        $('#FileFirmaKaseImza').val('');
    });

    // Banka / Not satırı ekle butonları (sadece UI tarafı, backend ile bağlanmadı)
    $('#btnBankEkle').off('click').on('click', function () {
        const $tbody = $('#tableXsltBank tbody');
        const rowHtml = `
            <tr>
                <td><input type="text" class="form-control" placeholder="Banka Adı" /></td>
                <td><input type="text" class="form-control" placeholder="Para Birimi" /></td>
                <td><input type="text" class="form-control" placeholder="IBAN" /></td>
                <td class="text-center">
                    <button type="button" class="btn btn-danger btn-xs btn-bank-del"><i class="fa fa-trash"></i></button>
                </td>
            </tr>`;
        $tbody.append(rowHtml);
        bindXsltBankRowEvents();
    });

    $('#btnNoteEkle').off('click').on('click', function () {
        const $tbody = $('#tableXsltNote tbody');
        const rowHtml = `
            <tr>
                <td><input type="text" class="form-control" placeholder="Not"></td>
                <td class="text-center">
                    <button type="button" class="btn btn-danger btn-xs btn-note-del"><i class="fa fa-trash"></i></button>
                </td>
            </tr>`;
        $tbody.append(rowHtml);
        bindXsltNoteRowEvents();
    });

    bindXsltBankRowEvents();
    bindXsltNoteRowEvents();
}

function bindXsltBankRowEvents() {
    $('#tableXsltBank tbody .btn-bank-del').off('click').on('click', function () {
        $(this).closest('tr').remove();
    });
}

function bindXsltNoteRowEvents() {
    $('#tableXsltNote tbody .btn-note-del').off('click').on('click', function () {
        $(this).closest('tr').remove();
    });
}

async function saveXsltTemplate() {
    if (!InvoiceDesignTemplateApi || !InvoiceDesignTemplateApi.create) {
        throw new Error('InvoiceDesignTemplateApi tanımlı değil.');
    }

    const name = ($('#XsltName').val() || '').trim();
    if (!name) {
        alert('XSLT Adı zorunludur.');
        return;
    }

    const templateType = ($('#xsltServiceType').val() || '1').toString();

    const base = buildBaseEntityForSave(SETTINGS_CURRENT_USER_ID, null);

    // DocumentTypeId opsiyonel bırakıldı; istersen xsltServiceType ile eşleştirebilirsin.
    const dto = {
        ...base,
        Id: 0,
        Name: name,
        Description: '',
        DocumentTypeId: null,
        TemplateType: templateType,
        Content: '',
        IsDefault: false
    };

    await InvoiceDesignTemplateApi.create(dto);
    showSuccess('XSLT tasarımı kaydedildi.');
}

/* ============================================================
   3) KULLANICILAR & ROLLER — sadece listeleme (şimdilik)
   ============================================================ */

async function initUsersRolesTab() {
    if (!$('#KullanicilarYetkiler').length) return;

    await Promise.all([
        loadUsersForSettings(),
        loadRolesForSettings()
    ]);

    // Modalların açılması vs. iskelet bırakıldı
    $('#btnUserAddMdl').off('click').on('click', function () {
        alert('Kullanıcı ekleme ekranı User.js üzerinden yönetilmektedir.');
    });

    $('#btnRolEkle').off('click').on('click', function () {
        alert('Rol ekleme için rol ekranını kullanınız.');
    });

    $('#btnYeniRolEkle').off('click').on('click', function () {
        alert('Rol ekleme için ayrı rol yönetim ekranı tasarlanmalıdır.');
    });
}

async function loadUsersForSettings() {
    if (!UserApi || !UserApi.list) return;
    try {
        const list = await UserApi.list();
        _usersCache = Array.isArray(list) ? list : [];

        const $userGrid = $('#grid_kullanici tbody');
        if ($userGrid.length) {
            $userGrid.empty();
            _usersCache.forEach(u => {
                const id = u.Id || u.id;
                const username = u.Username || u.username || '';
                $userGrid.append(`
                    <tr data-id="${id}">
                        <td>${username}</td>
                    </tr>
                `);
            });
        }
    } catch (err) {
        console.warn('[Settings] Kullanıcı listesi alınamadı:', err);
    }
}

async function loadRolesForSettings() {
    if (!RoleApi || !RoleApi.list) return;

    try {
        const list = await RoleApi.list();
        _rolesCache = Array.isArray(list) ? list : [];

        const $roleGrid = $('#gridRol tbody');
        if ($roleGrid.length) {
            $roleGrid.empty();
            _rolesCache.forEach(r => {
                const id = r.Id || r.id;
                const name = r.Name || r.name || '';
                const desc = r.Description || r.description || '';
                $roleGrid.append(`
                    <tr data-id="${id}">
                        <td class="text-center">
                            <input type="checkbox" class="role-select" data-id="${id}" />
                        </td>
                        <td>${name}</td>
                        <td>${desc}</td>
                    </tr>
                `);
            });
        }
    } catch (err) {
        console.warn('[Settings] Rol listesi alınamadı:', err);
    }
}

/* ============================================================
   4) E-MAIL AYARLARI — EmailSetting
   ============================================================ */

async function initEmailSettingsTab() {
    if (!$('#FtNoEmailAyarları').length) return;

    await loadEmailSettings();

    $('#emailAyarKaydetBtn').off('click').on('click', async function () {
        try {
            await saveEmailSettings();
        } catch (err) {
            showError(err);
        }
    });

    $('#emailAyarSilBtn').off('click').on('click', async function () {
        if (!_emailSetting || !(_emailSetting.Id || _emailSetting.id)) {
            alert('Silinecek bir e-posta ayarı bulunamadı.');
            return;
        }
        if (!confirm('E-posta ayarını silmek istiyor musunuz?')) return;

        try {
            const id = _emailSetting.Id || _emailSetting.id;
            await EmailSettingApi.remove(id);
            _emailSetting = null;
            await loadEmailSettings();
            showSuccess('E-posta ayarı silindi.');
        } catch (err) {
            showError(err);
        }
    });

    $('#btnTestMailGonder').off('click').on('click', function () {
        alert('Test e-posta gönderimi için backend endpoint\'i ile entegrasyon yapılmalıdır.');
    });
}

async function loadEmailSettings() {
    if (!EmailSettingApi || !EmailSettingApi.list) return;

    try {
        const list = await EmailSettingApi.list();
        _emailSetting = Array.isArray(list) && list.length ? list[0] : null;
    } catch (err) {
        console.warn('[Settings] E-posta ayarları alınamadı:', err);
        _emailSetting = null;
        return;
    }

    const s = _emailSetting;
    if (!s) {
        $('#txtHost').val('');
        $('#txtFormAdres').val('');
        $('#txtBcc').val('');
        $('#txtSmptKullaniciAdi').val('');
        $('#txtSmptSifre').val('');
        $('#txtSmptPort').val('');
        $('#sslChk').prop('checked', false);
        return;
    }

    $('#txtHost').val(prop(s, 'SmtpServer') || '');
    $('#txtFormAdres').val(prop(s, 'FromAddress') || '');
    $('#txtBcc').val(prop(s, 'Bcc') || '');
    $('#txtSmptKullaniciAdi').val(prop(s, 'UserName') || '');
    $('#txtSmptSifre').val(''); // güvenlik gereği doldurmuyoruz
    $('#txtSmptPort').val(prop(s, 'SmtpPort') || '');
    $('#sslChk').prop('checked', !!prop(s, 'EnableSsl'));
}

async function saveEmailSettings() {
    if (!EmailSettingApi || !EmailSettingApi.create) {
        throw new Error('EmailSettingApi tanımlı değil.');
    }

    const host = ($('#txtHost').val() || '').trim();
    const from = ($('#txtFormAdres').val() || '').trim();
    const portStr = ($('#txtSmptPort').val() || '').trim();

    if (!host) {
        alert('Host alanı zorunludur.');
        return;
    }
    if (!from) {
        alert('From Adresi zorunludur.');
        return;
    }

    const port = portStr ? Number(portStr) : 25;

    const existing = _emailSetting;
    const base = buildBaseEntityForSave(SETTINGS_CURRENT_USER_ID, existing);

    const dto = {
        ...base,
        Id: existing ? (existing.Id || existing.id) : 0,
        Name: 'Varsayılan',
        SmtpServer: host,
        SmtpPort: port,
        EnableSsl: $('#sslChk').is(':checked'),
        UserName: ($('#txtSmptKullaniciAdi').val() || '').trim(),
        Password: ($('#txtSmptSifre').val() || '').trim(),
        FromAddress: from,
        FromDisplayName: '',
        UseDefaultCredentials: false,
        IsDefault: true
        // Bcc backend entity'de yoksa ignore edilecek, varsa ekleyebilirsin
    };

    let saved;
    if (existing && (existing.Id || existing.id)) {
        const id = existing.Id || existing.id;
        saved = await EmailSettingApi.update(id, dto);
        showSuccess('E-posta ayarı güncellendi.');
    } else {
        saved = await EmailSettingApi.create(dto);
        showSuccess('E-posta ayarı kaydedildi.');
    }

    _emailSetting = saved;
}

/* ============================================================
   5) FATURA NO AYARLARI — InvoiceNumberSetting
   ============================================================ */

async function initInvoiceNumberTab() {
    if (!$('#FaturaNoAyarlari').length) return;

    await loadInvoiceNumberSettings();

    $('#btnAramaYap').off('click').on('click', function () {
        loadInvoiceNumberSettings();
    });

    $('#btnTemizle').off('click').on('click', function () {
        $('#SubeAdi').val('');
        $('#BelgeTur').val('');
        $('#BelgeOnEki').val('');
        $('#filterYil').val('');
        $('#SonBelgeSayisi').val('');
        loadInvoiceNumberSettings();
    });

    $('#PrefixOlustur').off('click').on('click', async function () {
        try {
            await createInvoiceNumberSettingFromFilters();
            await loadInvoiceNumberSettings();
        } catch (err) {
            showError(err);
        }
    });

    // Global fonksiyonlar: cshtml'de onclick ile çağrılıyor
    window.PrefixCodeEdit = async function (id, firmId) {
        try {
            await editInvoiceNumberSetting(id);
            await loadInvoiceNumberSettings();
        } catch (err) {
            showError(err);
        }
    };

    window.PrefixCodeDelete = async function (id, firmId) {
        try {
            await deleteInvoiceNumberSetting(id);
            await loadInvoiceNumberSettings();
        } catch (err) {
            showError(err);
        }
    };
}

function mapDocTypeToText(code) {
    const map = {
        1: 'E-Fatura',
        2: 'E-Arşiv',
        3: 'E-Arşiv İnternet Satışı',
        4: 'E-İrsaliye',
        5: 'E-SMM',
        6: 'E-Müstahsil',
        7: 'E-İrsaliye Yaniti',
        8: 'E-Gider Pusulası',
        9: 'E-Döviz Alım',
        10: 'E-Döviz Satış',
        11: 'E-Adisyon',
        12: 'E-Kıymetli Maden Alım',
        13: 'E-Kıymetli Maden Satış'
    };
    const num = Number(code);
    return map[num] || (code || '');
}

function getDocTypeIdFromEntity(entity) {
    if (!entity) return null;
    let id = prop(entity, 'DocumentTypeId');
    if (id) return Number(id);

    const dt = prop(entity, 'DocumentType');
    if (dt && typeof dt === 'object') {
        if (dt.Id || dt.id) return Number(dt.Id || dt.id);
    } else if (typeof dt === 'number') {
        return dt;
    }

    return null;
}

function getWarehouseNameFromEntity(entity) {
    if (!entity) return '';
    const whObj = prop(entity, 'Warehouse');
    if (whObj && typeof whObj === 'object') {
        return whObj.Name || whObj.name || '';
    }
    return prop(entity, 'WarehouseName') || '';
}

async function loadInvoiceNumberSettings() {
    if (!InvoiceNumberSettingApi || !InvoiceNumberSettingApi.list) return;

    const $tbody = $('#gridPrefixCode tbody');
    if (!$tbody.length) return;

    let list = [];
    try {
        const res = await InvoiceNumberSettingApi.list();
        list = Array.isArray(res) ? res : [];
    } catch (err) {
        console.warn('[Settings] InvoiceNumberSetting list alınamadı:', err);
        list = [];
    }

    const filterSube = ($('#SubeAdi').val() || '').trim().toLowerCase();
    const filterBelgeTur = ($('#BelgeTur').val() || '').trim();
    const filterPrefix = ($('#BelgeOnEki').val() || '').trim().toUpperCase();
    const filterYil = ($('#filterYil').val() || '').trim();
    const filterSonBelge = ($('#SonBelgeSayisi').val() || '').trim();

    // map'i temizle
    for (const k in _invoiceNumberMap) {
        if (Object.prototype.hasOwnProperty.call(_invoiceNumberMap, k)) {
            delete _invoiceNumberMap[k];
        }
    }

    const filtered = list.filter(s => {
        const warehouseName = getWarehouseNameFromEntity(s).toString().toLowerCase();
        if (filterSube && !warehouseName.includes(filterSube)) return false;

        const docTypeId = getDocTypeIdFromEntity(s);
        if (filterBelgeTur && docTypeId && Number(filterBelgeTur) !== Number(docTypeId)) return false;

        const prefix = (prop(s, 'Prefix') || '').toString().toUpperCase();
        if (filterPrefix && prefix !== filterPrefix) return false;

        const year = prop(s, 'Year') || '';
        if (filterYil && String(year) !== filterYil) return false;

        const lastNumber = prop(s, 'LastNumber') || 0;
        if (filterSonBelge && String(lastNumber) !== filterSonBelge) return false;

        return true;
    });

    $tbody.empty();

    filtered.forEach(s => {
        const id = s.Id || s.id;
        if (!id) return;
        _invoiceNumberMap[id] = s;

        const warehouseName = getWarehouseNameFromEntity(s) || '';
        const docTypeId = getDocTypeIdFromEntity(s) || '';
        const year = prop(s, 'Year') || '';
        const prefix = prop(s, 'Prefix') || '';
        const lastNumber = prop(s, 'LastNumber') || 0;

        const isPriority = !!(prop(s, 'IsUserScoped') || prop(s, 'IsDefault'));
        const priorityHtml = isPriority
            ? '<span class="label label-success">Öncelikli</span>'
            : '<span class="label label-default">Normal</span>';

        $tbody.append(`
            <tr>
                <td class="text-center">${warehouseName}</td>
                <td class="text-center">${mapDocTypeToText(docTypeId)}</td>
                <td class="text-center">${prefix}</td>
                <td class="text-center">${year}</td>
                <td class="text-center">${lastNumber}</td>
                <td class="text-center">${priorityHtml}</td>
                <td class="text-center">
                    <button class="btn btn-warning" title="Düzenle" onclick="PrefixCodeEdit(${id})"><i class="fa fa-edit"></i></button>
                    <button class="btn btn-danger" title="Sil" onclick="PrefixCodeDelete(${id})"><i class="fa fa-remove"></i></button>
                </td>
            </tr>
        `);
    });
}

/**
 * Yeni prefix oluşturma
 * Entity: InvoiceNumberSetting
 *  - DocumentTypeId (zorunlu)
 *  - Year (zorunlu)
 *  - Prefix (zorunlu)
 *  - LastNumber
 *  - WarehouseId (opsiyonel)
 *  - IsUserScoped, ScopedUserId (opsiyonel)
 */
async function createInvoiceNumberSettingFromFilters() {
    if (!InvoiceNumberSettingApi || !InvoiceNumberSettingApi.create) {
        throw new Error('InvoiceNumberSettingApi tanımlı değil.');
    }

    const subeAdi = ($('#SubeAdi').val() || '').trim(); // Şimdilik sadece ekranda gösterim için
    const belgeTurStr = ($('#BelgeTur').val() || '').trim();
    const prefix = ($('#BelgeOnEki').val() || '').trim().toUpperCase();
    const yilStr = ($('#filterYil').val() || '').trim();
    const sonBelgeStr = ($('#SonBelgeSayisi').val() || '').trim();

    if (!belgeTurStr) {
        alert('Belge Türü seçmelisiniz.');
        return;
    }
    if (!prefix) {
        alert('Belge Ön Eki zorunludur.');
        return;
    }
    if (!yilStr) {
        alert('Yıl zorunludur.');
        return;
    }

    const year = Number(yilStr);
    const lastNumber = sonBelgeStr ? Number(sonBelgeStr) : 0;
    const docTypeId = Number(belgeTurStr);

    const base = buildBaseEntityForSave(SETTINGS_CURRENT_USER_ID, null);

    const dto = {
        ...base,
        Id: 0,
        DocumentTypeId: docTypeId,
        WarehouseId: null,   // Şube seçimi için ileride Warehouse tablosu ile entegre edebilirsin
        Year: year,
        Prefix: prefix,
        LastNumber: lastNumber,
        IsUserScoped: false,
        ScopedUserId: null
        // Navigation alanlar (DocumentType, Warehouse, ScopedUser) GÖNDERİLMİYOR!
    };

    await InvoiceNumberSettingApi.create(dto);
    showSuccess('Yeni fatura numara ayarı oluşturuldu.');
}

async function editInvoiceNumberSetting(id) {
    if (!id) return;
    if (!InvoiceNumberSettingApi || !InvoiceNumberSettingApi.update) {
        throw new Error('InvoiceNumberSettingApi tanımlı değil.');
    }

    let entity = _invoiceNumberMap[id];
    if (!entity) {
        entity = await InvoiceNumberSettingApi.get(id);
    }
    if (!entity) return;

    const currentPrefix = prop(entity, 'Prefix') || '';
    const currentYear = prop(entity, 'Year') || new Date().getFullYear();
    const currentLast = prop(entity, 'LastNumber') || 0;

    const newPrefix = prompt('Belge Ön Eki', currentPrefix);
    if (newPrefix === null) return;

    const newYearStr = prompt('Yıl', String(currentYear));
    if (newYearStr === null) return;
    const newYear = Number(newYearStr) || currentYear;

    const newLastStr = prompt('Son Belge Sayısı', String(currentLast));
    if (newLastStr === null) return;
    const newLast = Number(newLastStr) || currentLast;

    const base = buildBaseEntityForSave(SETTINGS_CURRENT_USER_ID, entity);

    const docTypeId = getDocTypeIdFromEntity(entity);
    const warehouseId = prop(entity, 'WarehouseId') || null;
    const isUserScoped = prop(entity, 'IsUserScoped') || false;
    const scopedUserId = prop(entity, 'ScopedUserId') || null;

    const dto = {
        ...base,
        Id: entity.Id || entity.id,
        DocumentTypeId: docTypeId,
        WarehouseId: warehouseId,
        Year: newYear,
        Prefix: newPrefix.toUpperCase(),
        LastNumber: newLast,
        IsUserScoped: !!isUserScoped,
        ScopedUserId: scopedUserId
        // Navigation alanlar (DocumentType, Warehouse, ScopedUser) GÖNDERİLMİYOR!
    };

    const saved = await InvoiceNumberSettingApi.update(dto.Id, dto);
    _invoiceNumberMap[dto.Id] = saved;
    showSuccess('Fatura numara ayarı güncellendi.');
}

async function deleteInvoiceNumberSetting(id) {
    if (!id) return;
    if (!InvoiceNumberSettingApi || !InvoiceNumberSettingApi.remove) {
        throw new Error('InvoiceNumberSettingApi tanımlı değil.');
    }

    if (!confirm('Bu fatura numara ayarını silmek istiyor musunuz?')) return;

    await InvoiceNumberSettingApi.remove(id);
    delete _invoiceNumberMap[id];
    showSuccess('Fatura numara ayarı silindi.');
}

/* ============================================================
   6) BİLDİRİM AYARLARI — NotificationSetting (basit CRUD)
   ============================================================ */

async function initNotificationTab() {
    if (!$('#BildirimAyarlari').length) return;

    await loadNotificationSettings();

    $('#btnNotRuleEkle').off('click').on('click', async function () {
        try {
            await createNotificationSettingFromForm();
            await loadNotificationSettings();
        } catch (err) {
            showError(err);
        }
    });
}

async function loadNotificationSettings() {
    if (!NotificationSettingApi || !NotificationSettingApi.list) return;

    const $tbody = $('#alertInformationGrid tbody');
    if (!$tbody.length) return;

    let list = [];
    try {
        const res = await NotificationSettingApi.list();
        list = Array.isArray(res) ? res : [];
    } catch (err) {
        console.warn('[Settings] NotificationSetting list alınamadı:', err);
        list = [];
    }

    $tbody.empty();

    list.forEach(n => {
        const id = n.Id || n.id;
        const name = prop(n, 'Name') || '';
        const eventKey = prop(n, 'EventKey') || '';
        const targets = prop(n, 'Targets') || '';
        const sendEmail = !!prop(n, 'SendEmail');

        $tbody.append(`
            <tr data-id="${id}">
                <td>${eventKey || name}</td>
                <td>${sendEmail ? 'E-Posta' : ''}</td>
                <td>${targets}</td>
                <td class="text-center">
                    <button class="btn btn-danger btn-xs btn-not-del"><i class="fa fa-trash"></i></button>
                </td>
            </tr>
        `);
    });

    $('.btn-not-del').off('click').on('click', async function () {
        const id = $(this).closest('tr').data('id');
        if (!id) return;
        if (!confirm('Bu bildirim kuralını silmek istiyor musunuz?')) return;
        try {
            await NotificationSettingApi.remove(id);
            await loadNotificationSettings();
        } catch (err) {
            showError(err);
        }
    });
}

async function createNotificationSettingFromForm() {
    if (!NotificationSettingApi || !NotificationSettingApi.create) {
        throw new Error('NotificationSettingApi tanımlı değil.');
    }

    const allCustomer = $('#allCustomer').is(':checked');
    const senderVKN = ($('#senderVKN').val() || '').trim();
    const alertType = ($('#alertType').val() || 'EPOSTA').trim();
    const servType = ($('#servType').val() || '').trim();
    const email = ($('#ePosta').val() || '').trim();

    if (!email) {
        alert('E-Posta adresi zorunludur.');
        return;
    }

    const base = buildBaseEntityForSave(SETTINGS_CURRENT_USER_ID, null);

    const eventKey = servType === '4'
        ? 'INCOMING_EINVOICE_IRS'
        : 'INCOMING_EINVOICE';

    const targets = email; // ileride virgüllü liste yapabilirsin

    const dto = {
        ...base,
        Id: 0,
        Name: 'Gelen Fatura Uyarısı',
        EventKey: eventKey,
        SendEmail: true,
        SendSms: false,
        SendPush: false,
        WebhookUrl: '',
        Targets: targets,
        SubjectTemplate: 'Gelen e-belge uyarısı',
        BodyTemplate: `Gönderici: ${senderVKN || 'GENEL'}, Müşteri: ${allCustomer ? 'Tüm Müşteriler' : 'Seçili'}`,
    };

    await NotificationSettingApi.create(dto);
    showSuccess('Bildirim kuralı eklendi.');
}

/* ============================================================
   7) KULLANICI BAZLI GELEN FATURA — UserInvoiceInboxSetting
   ============================================================ */

async function initUserInvoiceInboxTab() {
    if (!$('#KullaniciBazliGelen').length) return;

    await loadUsersForInbox();
    await loadUserInvoiceInboxSettings();

    $('#btnInboxAdd').off('click').on('click', async function () {
        try {
            await saveUserInvoiceInboxSetting();
            await loadUserInvoiceInboxSettings();
        } catch (err) {
            showError(err);
        }
    });
}

async function loadUsersForInbox() {
    if (!UserApi || !UserApi.list) return;
    try {
        const list = await UserApi.list();
        _usersCache = Array.isArray(list) ? list : [];

        const $sel = $('#kullaniciBazliSelect');
        if ($sel.length) {
            $sel.empty();
            $sel.append(new Option('Seçiniz', ''));
            _usersCache.forEach(u => {
                const id = u.Id || u.id;
                const username = u.Username || u.username || '';
                const email = u.Email || u.email || '';
                const text = username || email || id;
                $sel.append(new Option(text, id));
            });
        }
    } catch (err) {
        console.warn('[Settings] Kullanıcı bazlı gelen için kullanıcı listesi alınamadı:', err);
    }
}

function findUserNameById(id) {
    if (!id || !_usersCache || !_usersCache.length) return '';
    const numId = Number(id);
    const u = _usersCache.find(x => (x.Id || x.id) === numId);
    if (!u) return '';
    return u.Username || u.username || u.Email || u.email || String(id);
}

async function loadUserInvoiceInboxSettings() {
    if (!UserInvoiceInboxSettingApi || !UserInvoiceInboxSettingApi.list) return;

    const $tbody = $('#alertInformationGrid2 tbody');
    if (!$tbody.length) return;

    let list = [];
    try {
        const res = await UserInvoiceInboxSettingApi.list();
        list = Array.isArray(res) ? res : [];
        _userInboxList = list;
    } catch (err) {
        console.warn('[Settings] Kullanıcı bazlı gelen fatura ayarları alınamadı:', err);
        list = [];
    }

    $tbody.empty();

    list.forEach(s => {
        const id = s.Id || s.id;
        const vkn = prop(s, 'VknTckn') || prop(s, 'InboxAlias') || '';
        const userId = prop(s, 'UserId') || prop(s, 'ScopedUserId') || null;
        const userName = findUserNameById(userId);

        $tbody.append(`
            <tr data-id="${id}">
                <td>${vkn}</td>
                <td>${userName || userId || ''}</td>
                <td class="text-center">
                    <button class="btn btn-danger btn-sm btn-inbox-del"><i class="fa fa-remove"></i></button>
                </td>
            </tr>
        `);
    });

    $('.btn-inbox-del').off('click').on('click', async function () {
        const id = $(this).closest('tr').data('id');
        if (!id) return;
        if (!confirm('Kaydı silmek istiyor musunuz?')) return;

        try {
            await UserInvoiceInboxSettingApi.remove(id);
            await loadUserInvoiceInboxSettings();
        } catch (err) {
            showError(err);
        }
    });
}

/**
 * Backend validasyonunda zorunlu alanlar:
 *   - InboxType
 *   - InboxAlias
 *   - Description
 */
async function saveUserInvoiceInboxSetting() {
    if (!UserInvoiceInboxSettingApi || !UserInvoiceInboxSettingApi.create) {
        throw new Error('UserInvoiceInboxSettingApi tanımlı değil.');
    }

    const vkn = ($('#gondericiVKN').val() || '').trim();
    const userIdStr = $('#kullaniciBazliSelect').val();

    if (!vkn) {
        alert('Alıcı VKN/TCKN zorunludur.');
        return;
    }
    if (!userIdStr) {
        alert('Kullanıcı seçmelisiniz.');
        return;
    }

    const userId = Number(userIdStr);
    const userName = findUserNameById(userId);

    // Bu entity'de UserId, BaseEntity'deki UserId alanı olarak hedef kullanıcıyı temsil ediyor
    const base = buildBaseEntityForSave(userId, null);

    const inboxType = 'EInvoice'; // varsayılan
    const inboxAlias = vkn;
    const description = userName
        ? `Kullanıcı bazlı gelen fatura (${userName})`
        : `Kullanıcı bazlı gelen fatura (UserId=${userId})`;

    const dto = {
        ...base,
        Id: 0,
        InboxAlias: inboxAlias,
        VknTckn: vkn,
        InboxType: inboxType,
        Description: description,
        IsDefault: false
    };

    await UserInvoiceInboxSettingApi.create(dto);
    showSuccess('Kullanıcı bazlı gelen fatura ayarı eklendi.');

    $('#gondericiVKN').val('');
}

/* ============================================================
   8) PARAMETRE AYARLARI — ParameterSetting
   ============================================================ */

async function initParameterTab() {
    if (!$('#ParametreAyarlari').length) return;

    await loadParameters();

    $('#btnParametreEkle').off('click').on('click', async function () {
        try {
            await createParameterFromPrompt();
            await loadParameters();
        } catch (err) {
            showError(err);
        }
    });

    $('#btnDegerEkle').off('click').on('click', function () {
        alert('Detaylı parametre değer ekranı için ek UI tasarlanmalıdır.');
    });
}

async function loadParameters() {
    if (!ParameterSettingApi || !ParameterSettingApi.list) return;

    const $tbody = $('#alertParametreGrid tbody');
    if (!$tbody.length) return;

    let list = [];
    try {
        const res = await ParameterSettingApi.list();
        list = Array.isArray(res) ? res : [];
        _parameterList = list;
    } catch (err) {
        console.warn('[Settings] Parametre ayarları alınamadı:', err);
        list = [];
    }

    $tbody.empty();

    list.forEach(p => {
        const name = prop(p, 'Name') || prop(p, 'Code') || '';
        const value = prop(p, 'Value') || '';

        $tbody.append(`
            <tr data-id="${p.Id || p.id}">
                <td>${name}</td>
                <td>${value}</td>
                <td colspan="9"></td>
            </tr>
        `);
    });
}

async function createParameterFromPrompt() {
    if (!ParameterSettingApi || !ParameterSettingApi.create) {
        throw new Error('ParameterSettingApi tanımlı değil.');
    }

    const paramName = ($('#parametreAdi').val() || '').trim();
    if (!paramName) {
        alert('Parametre Adı zorunludur.');
        return;
    }

    const value = prompt('Parametre değeri', '');
    if (value === null) return;

    const base = buildBaseEntityForSave(SETTINGS_CURRENT_USER_ID, null);

    const dto = {
        ...base,
        Id: 0,
        Group: '',
        Code: paramName,
        Name: paramName,
        Value: value,
        DataType: 'String'
    };

    await ParameterSettingApi.create(dto);
    showSuccess('Parametre eklendi.');
}

/* ============================================================
   9) KULLANICI DOĞRULAMA CİHAZLARI — UserVerificationDevice
   ============================================================ */

async function initDeviceTab() {
    if (!$('#CihazAyarlari').length) return;

    await loadUserVerificationDevices();

    $('#chkSelectAll').off('change').on('change', function () {
        const checked = $(this).is(':checked');
        $('#alertDeviceGrid tbody .chkDeviceRow').prop('checked', checked);
    });

    $('#btnBulkDelete').off('click').on('click', async function (e) {
        e.preventDefault();
        try {
            await bulkDeleteDevices();
        } catch (err) {
            showError(err);
        }
    });
}

async function loadUserVerificationDevices() {
    if (!UserVerificationDeviceApi || !UserVerificationDeviceApi.list) return;

    const $tbody = $('#alertDeviceGrid tbody');
    if (!$tbody.length) return;

    let list = [];
    try {
        const res = await UserVerificationDeviceApi.list();
        list = Array.isArray(res) ? res : [];
        _deviceList = list;
    } catch (err) {
        console.warn('[Settings] Kullanıcı doğrulama cihazları alınamadı:', err);
        list = [];
    }

    $tbody.empty();

    list.forEach(d => {
        const id = d.Id || d.id;
        const userName = prop(d, 'UserName') || findUserNameById(prop(d, 'UserId')) || '';
        const verifyType = prop(d, 'VerificationType') || prop(d, 'DeviceType') || '';
        const ip = prop(d, 'IpAddress') || '';
        const deviceInfo = prop(d, 'DeviceInfo') || prop(d, 'DeviceName') || '';
        const isTrusted = !!prop(d, 'IsTrusted');
        const statusText = isTrusted ? 'Güvenilir' : 'Onay Bekliyor';
        const statusClass = isTrusted ? 'label-success' : 'label-warning';
        const createdAt = prop(d, 'CreatedAt') || prop(d, 'CreatedDate') || '';

        $tbody.append(`
            <tr data-id="${id}">
                <td class="text-center">
                    <input type="checkbox" class="chkDeviceRow" data-id="${id}" />
                </td>
                <td class="text-center">
                    <button class="btn btn-danger btn-xs btn-device-del"><i class="fa fa-trash"></i></button>
                </td>
                <td>${userName}</td>
                <td>${verifyType}</td>
                <td>${ip}</td>
                <td>${deviceInfo}</td>
                <td><span class="label ${statusClass}">${statusText}</span></td>
                <td>${createdAt}</td>
            </tr>
        `);
    });

    $('.btn-device-del').off('click').on('click', async function () {
        const id = $(this).closest('tr').data('id');
        if (!id) return;
        if (!confirm('Bu cihaz kaydını silmek istiyor musunuz?')) return;

        try {
            await UserVerificationDeviceApi.remove(id);
            await loadUserVerificationDevices();
        } catch (err) {
            showError(err);
        }
    });
}

async function bulkDeleteDevices() {
    if (!UserVerificationDeviceApi || !UserVerificationDeviceApi.remove) {
        throw new Error('UserVerificationDeviceApi tanımlı değil.');
    }

    const ids = $('#alertDeviceGrid tbody .chkDeviceRow:checked')
        .map((i, el) => $(el).data('id'))
        .get()
        .filter(id => !!id);

    if (!ids.length) {
        alert('Silmek için en az bir cihaz seçmelisiniz.');
        return;
    }

    if (!confirm(`${ids.length} cihaz kaydı silinsin mi?`)) return;

    for (const id of ids) {
        try {
            await UserVerificationDeviceApi.remove(id);
        } catch (err) {
            console.warn('[Settings] Cihaz silme hatası (id=' + id + '):', err);
        }
    }

    await loadUserVerificationDevices();
    showSuccess('Seçili cihaz kayıtları silindi.');
}

/* ============================================================
   10) ÖZEL AYARLAR — Setting / Parametre (stub)
   ============================================================ */

async function initSpecialSettingsTab() {
    if (!$('#OzelAyarlar').length) return;

    // Sadece handler tanımlıyoruz; backend modeline göre genişletebilirsin
    window.TanimliAlanKaydet = async function (chk) {
        const $chk = $(chk);
        const row = $chk.closest('tr');
        const name = row.find('td:first').text().trim();
        const isChecked = $chk.is(':checked');

        console.log('[Settings] Özel ayar değişti:', name, '=>', isChecked);

        // Burada SettingApi veya ParameterSettingApi kullanarak kaydedebilirsin.
        // Örnek iskelet:
        /*
        try {
            const base = buildBaseEntityForSave(SETTINGS_CURRENT_USER_ID, null);
            const dto = {
                ...base,
                Id: 0,
                Code: name,
                Name: name,
                Value: isChecked ? '1' : '0',
                DataType: 'Bool',
                Group: 'OzelAyar'
            };
            await ParameterSettingApi.create(dto);
        } catch (err) {
            showError(err);
        }

        */
    };
}
