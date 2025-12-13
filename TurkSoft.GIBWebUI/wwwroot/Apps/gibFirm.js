import { FirmaApi, GibUserCreditAccountApi } from '../Entites/index.js';

/* ===========================================
   INIT GUARD (dosya 2 kere yüklenirse)
   =========================================== */
if (window.__gibFirmInitialized) {
    console.warn('[GibFirm] already initialized');
} else {
    window.__gibFirmInitialized = true;
}

/* ===========================================
   RowVersion yardımcıları
   =========================================== */
function rowVersionHexToBase64(hex) {
    if (!hex) return "";
    let h = String(hex).trim();
    if (h.startsWith("0x") || h.startsWith("0X")) h = h.substring(2);
    if (h.length % 2 === 1) h = "0" + h;

    const bytes = [];
    for (let i = 0; i < h.length; i += 2) {
        const b = parseInt(h.substr(i, 2), 16);
        if (!isNaN(b)) bytes.push(b);
    }

    let bin = "";
    for (let i = 0; i < bytes.length; i++) bin += String.fromCharCode(bytes[i]);

    try { return btoa(bin); }
    catch (e) {
        console.error("RowVersion base64 dönüşümünde hata:", e);
        return "";
    }
}

const DEFAULT_ROWVERSION_HEX = "0x00000000000007E1";
const DEFAULT_ROWVERSION_BASE64 = rowVersionHexToBase64(DEFAULT_ROWVERSION_HEX);

function getRowVersionFromEntityOrDefault(entity) {
    if (!entity) return DEFAULT_ROWVERSION_BASE64;

    let rv =
        entity.rowVersionBase64 || entity.RowVersionBase64 ||
        entity.rowVersion || entity.RowVersion ||
        entity.rowVersionHex || entity.RowVersionHex;

    if (rv && /^0x/i.test(rv)) rv = rowVersionHexToBase64(rv);
    return rv || DEFAULT_ROWVERSION_BASE64;
}

/* ===========================================
   BaseEntity builder (Firm + Child)
   =========================================== */
const BASE_USER_ID = 1;

function buildBaseEntityNew(nowIso) {
    return {
        UserId: BASE_USER_ID,
        IsActive: true,
        DeleteDate: null,
        DeletedByUserId: null,
        CreatedAt: nowIso,
        UpdatedAt: null,
        CreatedByUserId: BASE_USER_ID,
        UpdatedByUserId: BASE_USER_ID,
        RowVersion: DEFAULT_ROWVERSION_BASE64
    };
}

function buildBaseEntityUpdate(nowIso, existing, overrideRowVersion) {
    const ex = existing || {};
    const createdAt = ex.createdAt || ex.CreatedAt || nowIso;

    const isActive =
        (ex.isActive !== undefined ? ex.isActive :
            (ex.IsActive !== undefined ? ex.IsActive : true));

    const deleteDate = ex.deleteDate || ex.DeleteDate || null;
    const deletedByUserId = ex.deletedByUserId || ex.DeletedByUserId || null;

    const createdByUserId = ex.createdByUserId || ex.CreatedByUserId || BASE_USER_ID;
    const updatedByUserId = ex.updatedByUserId || ex.UpdatedByUserId || BASE_USER_ID;
    const userId = ex.userId || ex.UserId || BASE_USER_ID;

    const rowVersionBase64 = (overrideRowVersion && String(overrideRowVersion).trim())
        ? String(overrideRowVersion).trim()
        : getRowVersionFromEntityOrDefault(ex);

    return {
        UserId: userId,
        IsActive: isActive,
        DeleteDate: deleteDate,
        DeletedByUserId: deletedByUserId,
        CreatedAt: createdAt,
        UpdatedAt: nowIso,
        CreatedByUserId: createdByUserId,
        UpdatedByUserId: updatedByUserId,
        RowVersion: rowVersionBase64
    };
}

/* ===========================================
   Hizmet / Alias sabitleri (STRING)
   =========================================== */
const SERVICE_TYPES = [
    { value: 'EFATURA_EARSIV', text: 'E-Fatura / E-Arşiv' },
    { value: 'EIRSALIYE', text: 'E-İrsaliye' },
    { value: 'EDEFTER', text: 'E-Defter' },
    { value: 'EMM', text: 'E-MM' },
    { value: 'EBILET', text: 'E-Bilet' }
];

function buildServiceTypeSelect(selectedVal) {
    const v = String(selectedVal || 'EFATURA_EARSIV').trim();
    const opts = SERVICE_TYPES.map(x =>
        `<option value="${x.value}" ${x.value === v ? 'selected' : ''}>${x.text}</option>`
    ).join('');
    return `<select class="form-control svc-serviceType">${opts}</select>`;
}

function buildAliasServiceTypeSelect(selectedVal) {
    const v = String(selectedVal || 'EFATURA_EARSIV').trim();
    const opts = SERVICE_TYPES.map(x =>
        `<option value="${x.value}" ${x.value === v ? 'selected' : ''}>${x.text}</option>`
    ).join('');
    return `<select class="form-control alias-serviceType">${opts}</select>`;
}

/* ===========================================
   Tarih normalize (dd.MM.yyyy -> yyyy-MM-dd)
   =========================================== */
function normalizeDateToIso(value) {
    const s = String(value || '').trim();
    if (!s) return null;

    if (/^\d{4}-\d{2}-\d{2}$/.test(s)) return s;

    const m = s.match(/^(\d{2})[./-](\d{2})[./-](\d{4})$/);
    if (m) {
        const dd = m[1], mm = m[2], yyyy = m[3];
        return `${yyyy}-${mm}-${dd}`;
    }

    return s;
}

/* ===========================================
   MAIN INIT
   =========================================== */
function initGibFirm() {
    console.log('[GibFirm] init');

    const gridBody = document.querySelector('#tblGibFirms tbody');
    const form = document.getElementById('gibFirmForm');
    const btnSave = document.getElementById('btnSaveFirm');

    const $modal = (window.$ && window.$.fn && window.$.fn.modal)
        ? window.$('#gibFirmModal')
        : null;

    // ------- Firma form elemanları -------
    const fmId = document.getElementById('FirmId');
    const fmRowVersion = document.getElementById('RowVersionBase64') || document.getElementById('RowVersion');

    const fmTaxNo = document.getElementById('TaxNo');
    const fmCustomerName = document.getElementById('CustomerName');
    const fmTitle = document.getElementById('Title');
    const fmKepAddress = document.getElementById('KepAddress');
    const fmPersonalFirstName = document.getElementById('PersonalFirstName');
    const fmInstitutionType = document.getElementById('InstitutionType');
    const fmPersonalLastName = document.getElementById('PersonalLastName');
    const fmCorporateEmail = document.getElementById('CorporateEmail');
    const fmTaxOfficeProvince = document.getElementById('TaxOfficeProvince');
    const fmTaxOffice = document.getElementById('TaxOffice');
    const fmCustomerRepresentative = document.getElementById('CustomerRepresentative');

    // ✅ CSHTML’de eklenmeli (yoksa required hatası bitmez)
    const fmCommercialRegistrationNo = document.getElementById('CommercialRegistrationNo');
    const fmMersisNo = document.getElementById('MersisNo');

    const fmResponsibleTckn = document.getElementById('ResponsibleTckn');
    const fmResponsibleFirstName = document.getElementById('ResponsibleFirstName');
    const fmResponsibleLastName = document.getElementById('ResponsibleLastName');
    const fmResponsibleMobilePhone = document.getElementById('ResponsibleMobilePhone');
    const fmResponsibleEmail = document.getElementById('ResponsibleEmail');

    const fmCreatedByPersonFirstName = document.getElementById('CreatedByPersonFirstName');
    const fmCreatedByPersonLastName = document.getElementById('CreatedByPersonLastName');
    const fmCreatedByPersonMobilePhone = document.getElementById('CreatedByPersonMobilePhone');

    const fmAddress = document.getElementById('AddressLine');
    const fmCity = document.getElementById('City');
    const fmDistrict = document.getElementById('District');
    const fmCountry = document.getElementById('Country');
    const fmPostalCode = document.getElementById('PostalCode');
    const fmPhone = document.getElementById('Phone');
    const fmEmail = document.getElementById('FirmEmail');

    const fmGibAlias = document.getElementById('GibAlias');
    const fmApiKey = document.getElementById('ApiKey');
    const fmIsEInv = document.getElementById('IsEInvoiceRegistered');
    const fmIsEArch = document.getElementById('IsEArchiveRegistered');
    const fmInitialCredits = document.getElementById('InitialCredits');

    function getSvcBody() { return document.querySelector('#FirmServiceGrid tbody'); }
    function getAliasBody() { return document.querySelector('#FirmServiceAliasGrid tbody'); }

    let currentFirmEntity = null;

    let serviceRowSeq = 0;
    let aliasRowSeq = 0;
    let selectedServiceRowIndex = null;

    const serviceRowIndexByServiceId = {}; // edit modunda: serviceId -> rowIndex

    function nextServiceRowIndex() { serviceRowSeq += 1; return serviceRowSeq; }
    function nextAliasRowIndex() { aliasRowSeq += 1; return aliasRowSeq; }

    function clearServiceAliasTables() {
        serviceRowSeq = 0;
        aliasRowSeq = 0;
        selectedServiceRowIndex = null;
        Object.keys(serviceRowIndexByServiceId).forEach(k => delete serviceRowIndexByServiceId[k]);

        const svcBody = getSvcBody();
        const aliasBody = getAliasBody();
        if (svcBody) svcBody.innerHTML = '';
        if (aliasBody) aliasBody.innerHTML = '';
    }

    function getSelectedServiceRow() {
        const svcBody = getSvcBody();
        if (!svcBody) return null;
        return svcBody.querySelector('tr.active');
    }

    // ✅ Alias satırları ASLA kaybolmaz: sadece seçili satır dışı olanlar soluklaşır
    function refreshAliasVisibility() {
        const aliasBody = getAliasBody();
        if (!aliasBody) return;

        const selected = getSelectedServiceRow();
        const idx = selected ? String(selected.dataset.serviceRowIndex) : null;

        aliasBody.querySelectorAll('tr').forEach(tr => {
            tr.style.display = '';
            if (!idx) tr.style.opacity = '1';
            else tr.style.opacity = (String(tr.dataset.serviceRowIndex) === idx) ? '1' : '0.35';
        });
    }

    function findFirstServiceRowIndexByType(serviceType) {
        const svcBody = getSvcBody();
        if (!svcBody) return null;

        const rows = Array.from(svcBody.querySelectorAll('tr'));
        const found = rows.find(r => String(r.querySelector('.svc-serviceType')?.value || '') === String(serviceType));
        return found ? String(found.dataset.serviceRowIndex) : null;
    }

    function addServiceRow(service, options) {
        const opts = options || {};
        const makeActive = (opts.makeActive !== false);

        const svcBody = getSvcBody();
        if (!svcBody) return;

        const rowIndex = nextServiceRowIndex();

        const idVal = service ? (service.id || service.Id || 0) : 0;
        const serviceTypeVal = service
            ? String(service.serviceType || service.ServiceType || 'EFATURA_EARSIV')
            : 'EFATURA_EARSIV';

        let startDate = service ? (service.startDate || service.StartDate || '') : '';
        let endDate = service ? (service.endDate || service.EndDate || '') : '';
        let tariffType = service ? (service.tariffType || service.TariffType || '') : '';
        let status = service ? (service.status || service.Status || '') : '';

        // Server tarafında string required çıkarsa boş kalmasın:
        if (!tariffType) tariffType = 'Kontör';
        if (!status) status = 'Aktif';

        if (idVal) serviceRowIndexByServiceId[idVal] = rowIndex;

        const tr = document.createElement('tr');
        tr.dataset.serviceRowIndex = String(rowIndex);
        tr.innerHTML = `
            <td>
                <input type="hidden" class="svc-id" value="${idVal}" />
                ${buildServiceTypeSelect(serviceTypeVal)}
            </td>
            <td><input type="text" class="form-control svc-startDate" placeholder="GG.AA.YYYY" value="${startDate || ''}" /></td>
            <td><input type="text" class="form-control svc-endDate" placeholder="GG.AA.YYYY" value="${endDate || ''}" /></td>
            <td><input type="text" class="form-control svc-tariffType" value="${tariffType || ''}" /></td>
            <td>
                <select class="form-control svc-status">
                    <option value="Aktif" ${status === 'Aktif' ? 'selected' : ''}>Aktif</option>
                    <option value="Pasif" ${status === 'Pasif' ? 'selected' : ''}>Pasif</option>
                </select>
            </td>
            <td class="text-center">
                <button type="button" class="btn btn-xs btn-danger act-del-service">
                    <i class="fa fa-trash"></i>
                </button>
            </td>`;

        svcBody.appendChild(tr);

        if (makeActive) {
            svcBody.querySelectorAll('tr').forEach(r => r.classList.remove('active'));
            tr.classList.add('active');
            selectedServiceRowIndex = rowIndex;
        }

        refreshAliasVisibility();
    }

    function addAliasRow(alias, serviceRowIndex, serviceTypeVal) {
        const aliasBody = getAliasBody();
        if (!aliasBody) return;

        // Eğer serviceRowIndex gelmediyse seçili service row’u baz al
        if (!serviceRowIndex) {
            const selRow = getSelectedServiceRow();
            if (!selRow) { alert('Önce yukarıdaki listeden bir hizmet satırı seçin.'); return; }
            serviceRowIndex = selRow.dataset.serviceRowIndex;
        }

        const rowIndex = nextAliasRowIndex();
        const idVal = alias ? (alias.id || alias.Id || 0) : 0;

        const directionVal = alias ? (alias.direction || alias.Direction || 'SENDER') : 'SENDER';
        const aliasText = alias ? (alias.alias || alias.Alias || '') : '';

        const svcType = String(serviceTypeVal || (alias ? (alias.serviceType || alias.ServiceType || 'EFATURA_EARSIV') : 'EFATURA_EARSIV'));

        const tr = document.createElement('tr');
        tr.dataset.aliasRowIndex = String(rowIndex);
        tr.dataset.serviceRowIndex = String(serviceRowIndex);

        tr.innerHTML = `
            <td>${buildAliasServiceTypeSelect(svcType)}</td>
            <td>
                <input type="hidden" class="alias-id" value="${idVal}" />
                <select class="form-control alias-direction">
                    <option value="SENDER" ${String(directionVal) === 'SENDER' ? 'selected' : ''}>Gönderici</option>
                    <option value="RECEIVER" ${String(directionVal) === 'RECEIVER' ? 'selected' : ''}>Alıcı</option>
                </select>
            </td>
            <td><input type="text" class="form-control alias-value" value="${aliasText || ''}" /></td>
            <td class="text-center">
                <button type="button" class="btn btn-xs btn-danger act-del-alias">
                    <i class="fa fa-trash"></i>
                </button>
            </td>`;

        aliasBody.appendChild(tr);
        refreshAliasVisibility();
    }

    function loadServicesAndAliasesFromFirm(firm) {
        clearServiceAliasTables();

        const svcBody = getSvcBody();
        const aliasBody = getAliasBody();
        if (!svcBody || !aliasBody) return;

        // 1) Services
        const services = (firm && (firm.services || firm.Services)) || [];
        if (Array.isArray(services)) {
            services.forEach(s => addServiceRow(s, { makeActive: false }));
        }

        // 2) Aliases (öncelik: Services içinden)
        if (Array.isArray(services)) {
            services.forEach(s => {
                const sid = s.id || s.Id || 0;
                const rowIdx = sid ? serviceRowIndexByServiceId[sid] : null;
                const svcType = s.serviceType || s.ServiceType || 'EFATURA_EARSIV';
                const aliases = (s.aliases || s.Aliases) || [];
                if (!rowIdx) return;

                if (Array.isArray(aliases)) {
                    aliases.forEach(a => addAliasRow(a, rowIdx, svcType));
                }
            });
        }

        // 3) Eğer API root seviyede Aliases döndürüyorsa (geri uyum)
        const rootAliases = (firm && (firm.aliases || firm.Aliases)) || [];
        if (Array.isArray(rootAliases) && rootAliases.length) {
            rootAliases.forEach(a => {
                const sid =
                    a.gibFirmServiceId || a.GibFirmServiceId ||
                    a.serviceId || a.ServiceId || 0;

                const rowIdx = sid ? serviceRowIndexByServiceId[sid] : null;
                const svcType = a.serviceType || a.ServiceType || 'EFATURA_EARSIV';
                if (!rowIdx) return;
                addAliasRow(a, rowIdx, svcType);
            });
        }

        // ilk service satırını seç
        const firstRow = svcBody.querySelector('tr');
        svcBody.querySelectorAll('tr').forEach(r => r.classList.remove('active'));
        if (firstRow) {
            firstRow.classList.add('active');
            selectedServiceRowIndex = firstRow.dataset.serviceRowIndex;
        } else {
            selectedServiceRowIndex = null;
        }

        refreshAliasVisibility();
    }

    /* ========= Ana grid ========= */
    async function loadTable() {
        if (!gridBody) return;

        try {
            const data = await FirmaApi.list();
            gridBody.innerHTML = (data || []).map(f => `
                <tr data-id="${f.id || f.Id}">
                    <td>${f.title || f.Title || ''}</td>
                    <td>${f.taxNo || f.TaxNo || ''}</td>
                    <td>${f.gibAlias || f.GibAlias || ''}</td>
                    <td>${(f.isEInvoiceRegistered || f.IsEInvoiceRegistered) ? 'Evet' : 'Hayır'}</td>
                    <td>${(f.isEArchiveRegistered || f.IsEArchiveRegistered) ? 'Evet' : 'Hayır'}</td>
                    <td class="text-center">
                        <button class="btn btn-warning btn-sm act-edit-firm" type="button"><i class="fa fa-edit"></i></button>
                        <button class="btn btn-danger btn-sm act-del-firm" type="button"><i class="fa fa-trash"></i></button>
                    </td>
                </tr>
            `).join('');
        } catch (err) {
            console.error('Firma listesi yüklenemedi:', err);
            alert(err.message || 'Firma listesi yüklenemedi.');
        }
    }

    function clearForm() {
        currentFirmEntity = null;

        if (fmId) fmId.value = '';
        if (fmRowVersion) fmRowVersion.value = DEFAULT_ROWVERSION_BASE64;

        const clear = (el, val = '') => { if (el) el.value = val; };

        clear(fmTaxNo);
        clear(fmCustomerName);
        clear(fmTitle);
        clear(fmKepAddress);
        clear(fmPersonalFirstName);
        if (fmInstitutionType) fmInstitutionType.value = '1';
        clear(fmPersonalLastName);
        clear(fmCorporateEmail);
        clear(fmTaxOfficeProvince);
        clear(fmTaxOffice);
        clear(fmCustomerRepresentative);

        clear(fmCommercialRegistrationNo);
        clear(fmMersisNo);

        clear(fmResponsibleTckn);
        clear(fmResponsibleFirstName);
        clear(fmResponsibleLastName);
        clear(fmResponsibleMobilePhone);
        clear(fmResponsibleEmail);

        clear(fmCreatedByPersonFirstName);
        clear(fmCreatedByPersonLastName);
        clear(fmCreatedByPersonMobilePhone);

        if (fmAddress) fmAddress.value = '';
        clear(fmCity);
        clear(fmDistrict);
        clear(fmCountry);
        clear(fmPostalCode);
        clear(fmPhone);
        clear(fmEmail);

        clear(fmGibAlias);
        clear(fmApiKey);

        if (fmIsEInv) fmIsEInv.checked = false;
        if (fmIsEArch) fmIsEArch.checked = false;

        if (fmInitialCredits) fmInitialCredits.value = '';

        clearServiceAliasTables();
    }

    function bindGridRowEvents() {
        if (!gridBody) return;

        gridBody.querySelectorAll('.act-edit-firm').forEach(btn => {
            btn.addEventListener('click', async () => {
                const id = Number(btn.closest('tr')?.dataset.id || 0);
                if (!id) return;

                try {
                    const f = await FirmaApi.get(id);
                    currentFirmEntity = f;

                    clearForm();

                    if (fmId) fmId.value = f.id || f.Id;
                    if (fmRowVersion) fmRowVersion.value = getRowVersionFromEntityOrDefault(f);

                    if (fmTaxNo) fmTaxNo.value = f.taxNo || f.TaxNo || '';
                    if (fmCustomerName) fmCustomerName.value = f.customerName || f.CustomerName || '';
                    if (fmTitle) fmTitle.value = f.title || f.Title || '';
                    if (fmKepAddress) fmKepAddress.value = f.kepAddress || f.KepAddress || '';
                    if (fmPersonalFirstName) fmPersonalFirstName.value = f.personalFirstName || f.PersonalFirstName || '';
                    if (fmInstitutionType) fmInstitutionType.value = String(f.institutionType || f.InstitutionType || 1);
                    if (fmPersonalLastName) fmPersonalLastName.value = f.personalLastName || f.PersonalLastName || '';
                    if (fmCorporateEmail) fmCorporateEmail.value = f.corporateEmail || f.CorporateEmail || '';
                    if (fmTaxOfficeProvince) fmTaxOfficeProvince.value = f.taxOfficeProvince || f.TaxOfficeProvince || '';
                    if (fmTaxOffice) fmTaxOffice.value = f.taxOffice || f.TaxOffice || '';
                    if (fmCustomerRepresentative) fmCustomerRepresentative.value = f.customerRepresentative || f.CustomerRepresentative || '';

                    if (fmCommercialRegistrationNo) fmCommercialRegistrationNo.value = f.commercialRegistrationNo || f.CommercialRegistrationNo || '';
                    if (fmMersisNo) fmMersisNo.value = f.mersisNo || f.MersisNo || '';

                    if (fmResponsibleTckn) fmResponsibleTckn.value = f.responsibleTckn || f.ResponsibleTckn || '';
                    if (fmResponsibleFirstName) fmResponsibleFirstName.value = f.responsibleFirstName || f.ResponsibleFirstName || '';
                    if (fmResponsibleLastName) fmResponsibleLastName.value = f.responsibleLastName || f.ResponsibleLastName || '';
                    if (fmResponsibleMobilePhone) fmResponsibleMobilePhone.value = f.responsibleMobilePhone || f.ResponsibleMobilePhone || '';
                    if (fmResponsibleEmail) fmResponsibleEmail.value = f.responsibleEmail || f.ResponsibleEmail || '';

                    if (fmCreatedByPersonFirstName) fmCreatedByPersonFirstName.value = f.createdByPersonFirstName || f.CreatedByPersonFirstName || '';
                    if (fmCreatedByPersonLastName) fmCreatedByPersonLastName.value = f.createdByPersonLastName || f.CreatedByPersonLastName || '';
                    if (fmCreatedByPersonMobilePhone) fmCreatedByPersonMobilePhone.value = f.createdByPersonMobilePhone || f.CreatedByPersonMobilePhone || '';

                    if (fmAddress) fmAddress.value = f.addressLine || f.AddressLine || '';
                    if (fmCity) fmCity.value = f.city || f.City || '';
                    if (fmDistrict) fmDistrict.value = f.district || f.District || '';
                    if (fmCountry) fmCountry.value = f.country || f.Country || '';
                    if (fmPostalCode) fmPostalCode.value = f.postalCode || f.PostalCode || '';
                    if (fmPhone) fmPhone.value = f.phone || f.Phone || '';
                    if (fmEmail) fmEmail.value = f.email || f.Email || '';

                    if (fmGibAlias) fmGibAlias.value = f.gibAlias || f.GibAlias || '';
                    if (fmApiKey) fmApiKey.value = f.apiKey || f.ApiKey || '';
                    if (fmIsEInv) fmIsEInv.checked = !!(f.isEInvoiceRegistered || f.IsEInvoiceRegistered);
                    if (fmIsEArch) fmIsEArch.checked = !!(f.isEArchiveRegistered || f.IsEArchiveRegistered);

                    loadServicesAndAliasesFromFirm(f);

                    const titleEl = document.getElementById('gibFirmModalLabel');
                    if (titleEl) titleEl.textContent = 'Firma Güncelle';

                    if ($modal) $modal.modal('show');
                } catch (err) {
                    console.error('Firma getirilemedi:', err);
                    alert(err.message || 'Firma getirilemedi.');
                }
            });
        });

        gridBody.querySelectorAll('.act-del-firm').forEach(btn => {
            btn.addEventListener('click', async () => {
                const id = Number(btn.closest('tr')?.dataset.id || 0);
                if (!id) return;
                if (!confirm('Bu firma silinsin mi?')) return;

                try {
                    await FirmaApi.remove(id);
                    await loadTable();
                    bindGridRowEvents();
                } catch (err) {
                    console.error('Firma silinemedi:', err);
                    alert(err.message || 'Firma silinemedi.');
                }
            });
        });
    }

    /* ========= Delegated Events ========= */

    // Inline onclick kalmışsa hata vermesin:
    window.__gib_addServiceRow = (e) => { if (e?.preventDefault) e.preventDefault(); addServiceRow(null, { makeActive: true }); };
    window.__gib_addServiceAlias = (e) => {
        if (e?.preventDefault) e.preventDefault();
        const selected = getSelectedServiceRow();
        if (!selected) { alert('Önce yukarıdaki listeden bir hizmet satırı seçin.'); return; }
        const idx = selected.dataset.serviceRowIndex;
        const svcTypeVal = selected.querySelector('.svc-serviceType')?.value || 'EFATURA_EARSIV';
        addAliasRow(null, idx, svcTypeVal);
    };

    document.addEventListener('click', async (e) => {
        // Yeni Firma
        if (e.target.closest('#btnNewFirm')) {
            clearForm();
            const titleEl = document.getElementById('gibFirmModalLabel');
            if (titleEl) titleEl.textContent = 'Yeni Firma';
            if ($modal) $modal.modal('show');
            return;
        }

        // Hizmet Ekle
        if (e.target.closest('#btnNewFirmService')) {
            window.__gib_addServiceRow(e);
            return;
        }

        // Alias Ekle
        if (e.target.closest('#btnNewFirmServiceAlias')) {
            window.__gib_addServiceAlias(e);
            return;
        }

        // Hizmet grid içi
        const svcBody = getSvcBody();
        if (svcBody && svcBody.contains(e.target)) {
            const tr = e.target.closest('tr');
            if (!tr) return;

            // sil
            const delBtn = e.target.closest('.act-del-service');
            if (delBtn) {
                const idx = tr.dataset.serviceRowIndex;
                tr.remove();

                // bağlı aliasları SİLMEK İSTEMİYORSAN burayı yorum satırı yapabilirsin.
                // Şimdilik mantıklı ilişki için: service silinince bağlı aliaslar da silinir.
                const aliasBody = getAliasBody();
                if (aliasBody) {
                    Array.from(aliasBody.querySelectorAll('tr')).forEach(r => {
                        if (String(r.dataset.serviceRowIndex) === String(idx)) r.remove();
                    });
                }

                // yeni aktif satır
                const firstRow = svcBody.querySelector('tr');
                svcBody.querySelectorAll('tr').forEach(r => r.classList.remove('active'));
                if (firstRow) {
                    firstRow.classList.add('active');
                    selectedServiceRowIndex = firstRow.dataset.serviceRowIndex;
                } else {
                    selectedServiceRowIndex = null;
                }

                refreshAliasVisibility();
                return;
            }

            // satır seç
            svcBody.querySelectorAll('tr').forEach(r => r.classList.remove('active'));
            tr.classList.add('active');
            selectedServiceRowIndex = tr.dataset.serviceRowIndex;
            refreshAliasVisibility();
            return;
        }

        // Alias grid içi (sil)
        const aliasBody = getAliasBody();
        if (aliasBody && aliasBody.contains(e.target)) {
            const delAliasBtn = e.target.closest('.act-del-alias');
            if (delAliasBtn) {
                delAliasBtn.closest('tr')?.remove();
                refreshAliasVisibility();
            }
        }
    });

    document.addEventListener('change', (e) => {
        // ServiceType değiştiyse: bu service satırına bağlı alias satırlarının serviceType’ını güncelle
        const svcSel = e.target.closest('.svc-serviceType');
        if (svcSel) {
            const svcRow = svcSel.closest('tr');
            if (!svcRow) return;

            const rowIndex = String(svcRow.dataset.serviceRowIndex);
            const newType = String(svcSel.value);

            const aliasBody = getAliasBody();
            if (aliasBody) {
                aliasBody.querySelectorAll(`tr[data-service-row-index="${rowIndex}"]`).forEach(aRow => {
                    const aSel = aRow.querySelector('.alias-serviceType');
                    if (aSel) aSel.value = newType;
                });
            }

            refreshAliasVisibility();
            return;
        }

        // Alias serviceType değiştiyse: alias satırını o tipe sahip bir service satırına bağla (yoksa yeni service aç)
        const aliasSel = e.target.closest('.alias-serviceType');
        if (aliasSel) {
            const aRow = aliasSel.closest('tr');
            if (!aRow) return;

            const newType = String(aliasSel.value);
            let targetRowIndex = findFirstServiceRowIndexByType(newType);

            if (!targetRowIndex) {
                // yoksa yeni service satırı aç ama diğerlerini bozma
                addServiceRow({ ServiceType: newType, TariffType: 'Kontör', Status: 'Aktif' }, { makeActive: false });
                targetRowIndex = findFirstServiceRowIndexByType(newType);
            }

            if (targetRowIndex) {
                aRow.dataset.serviceRowIndex = String(targetRowIndex);
            }

            refreshAliasVisibility();
        }
    });

    /* ========= Submit ========= */
    let isSaving = false;

    function val(el) { return String(el?.value || '').trim(); }

    function requireFields(list) {
        const missing = [];
        for (const item of list) {
            const v = val(item.el);
            if (!v) missing.push(item.name);
        }
        return missing;
    }

    if (form) {
        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            if (isSaving) return;

            // CSHTML’de yoksa zaten dolduramaz
            if (!fmCommercialRegistrationNo || !fmMersisNo) {
                alert('CSHTML’de CommercialRegistrationNo ve MersisNo inputları yok. Ekleyip tekrar dene.');
                return;
            }

            // ✅ required kontroller (API’nin şikayet ettiği alanlar)
            const missing = requireFields([
                { name: 'Title', el: fmTitle },
                { name: 'TaxNo', el: fmTaxNo },
                { name: 'CustomerName', el: fmCustomerName },
                { name: 'CorporateEmail', el: fmCorporateEmail },
                { name: 'TaxOfficeProvince', el: fmTaxOfficeProvince },
                { name: 'TaxOffice', el: fmTaxOffice },
                { name: 'CustomerRepresentative', el: fmCustomerRepresentative },

                { name: 'CommercialRegistrationNo', el: fmCommercialRegistrationNo },
                { name: 'MersisNo', el: fmMersisNo },

                { name: 'ResponsibleTckn', el: fmResponsibleTckn },
                { name: 'ResponsibleFirstName', el: fmResponsibleFirstName },
                { name: 'ResponsibleLastName', el: fmResponsibleLastName },
                { name: 'ResponsibleMobilePhone', el: fmResponsibleMobilePhone },
                { name: 'ResponsibleEmail', el: fmResponsibleEmail },

                { name: 'CreatedByPersonFirstName', el: fmCreatedByPersonFirstName },
                { name: 'CreatedByPersonLastName', el: fmCreatedByPersonLastName },
                { name: 'CreatedByPersonMobilePhone', el: fmCreatedByPersonMobilePhone },
            ]);

            if (missing.length) {
                alert('Zorunlu alanlar boş: ' + missing.join(', '));
                return;
            }

            isSaving = true;
            if (btnSave) btnSave.disabled = true;

            const isNew = !fmId || !fmId.value;
            const nowIso = new Date().toISOString();

            const creditsRaw = val(fmInitialCredits);
            const credits = creditsRaw ? parseInt(creditsRaw, 10) : 0;

            const overrideRv = val(fmRowVersion) || null;

            const baseEntity = isNew
                ? buildBaseEntityNew(nowIso)
                : buildBaseEntityUpdate(nowIso, currentFirmEntity, overrideRv);

            const dto = {
                ...baseEntity,
                Id: isNew ? 0 : Number(fmId.value),

                Title: val(fmTitle),
                TaxNo: val(fmTaxNo),
                CustomerName: val(fmCustomerName),

                TaxOffice: val(fmTaxOffice),
                TaxOfficeProvince: val(fmTaxOfficeProvince),
                CommercialRegistrationNo: val(fmCommercialRegistrationNo),
                MersisNo: val(fmMersisNo),

                KepAddress: val(fmKepAddress),
                PersonalFirstName: val(fmPersonalFirstName),
                PersonalLastName: val(fmPersonalLastName),
                InstitutionType: fmInstitutionType ? parseInt(fmInstitutionType.value || '0', 10) : 0,
                CustomerRepresentative: val(fmCustomerRepresentative),

                CorporateEmail: val(fmCorporateEmail),

                ResponsibleTckn: val(fmResponsibleTckn),
                ResponsibleFirstName: val(fmResponsibleFirstName),
                ResponsibleLastName: val(fmResponsibleLastName),
                ResponsibleMobilePhone: val(fmResponsibleMobilePhone),
                ResponsibleEmail: val(fmResponsibleEmail),

                CreatedByPersonFirstName: val(fmCreatedByPersonFirstName),
                CreatedByPersonLastName: val(fmCreatedByPersonLastName),
                CreatedByPersonMobilePhone: val(fmCreatedByPersonMobilePhone),

                AddressLine: val(fmAddress),
                City: val(fmCity),
                District: val(fmDistrict),
                Country: val(fmCountry),
                PostalCode: val(fmPostalCode),
                Phone: val(fmPhone),
                Email: val(fmEmail),

                GibAlias: val(fmGibAlias),
                ApiKey: val(fmApiKey),
                IsEInvoiceRegistered: !!fmIsEInv?.checked,
                IsEArchiveRegistered: !!fmIsEArch?.checked
            };

            // Services + Aliases (NESTED)
            const servicesDto = [];
            const svcBody = getSvcBody();
            const aliasBody = getAliasBody();

            // Önce Service satırlarını al
            const serviceRows = svcBody ? Array.from(svcBody.querySelectorAll('tr')) : [];
            for (const tr of serviceRows) {
                const idVal = parseInt((tr.querySelector('.svc-id')?.value || '0'), 10) || 0;

                const serviceTypeVal = String(tr.querySelector('.svc-serviceType')?.value || '').trim();
                const startDate = normalizeDateToIso(val(tr.querySelector('.svc-startDate')));
                const endDate = normalizeDateToIso(val(tr.querySelector('.svc-endDate')));
                let tariffType = val(tr.querySelector('.svc-tariffType'));
                let status = String(tr.querySelector('.svc-status')?.value || '').trim();

                if (!tariffType) tariffType = 'Kontör';
                if (!status) status = 'Aktif';

                if (!serviceTypeVal) continue;

                const childBase = buildBaseEntityNew(nowIso);

                servicesDto.push({
                    ...childBase,
                    Id: idVal,
                    GibFirmId: dto.Id || 0,
                    ServiceType: serviceTypeVal,
                    StartDate: startDate || null,
                    EndDate: endDate || null,
                    TariffType: tariffType,
                    Status: status,
                    Aliases: [] // buraya aliasları bağlayacağız
                });
            }

            // Aliasları ilgili serviceRowIndex’e göre service’a bağla
            const aliasRows = aliasBody ? Array.from(aliasBody.querySelectorAll('tr')) : [];
            for (const tr of aliasRows) {
                const aliasText = val(tr.querySelector('.alias-value'));
                if (!aliasText) continue;

                const idVal = parseInt((tr.querySelector('.alias-id')?.value || '0'), 10) || 0;

                const directionVal = String(tr.querySelector('.alias-direction')?.value || 'SENDER');
                const aliasServiceTypeVal = String(tr.querySelector('.alias-serviceType')?.value || 'EFATURA_EARSIV').trim();

                const svcRowIndex = String(tr.dataset.serviceRowIndex || '');

                // service satırını bul: önce rowIndex’ten, yoksa serviceType’tan
                let service = null;

                if (svcRowIndex) {
                    const svcRow = serviceRows.find(r => String(r.dataset.serviceRowIndex) === svcRowIndex);
                    if (svcRow) {
                        const st = String(svcRow.querySelector('.svc-serviceType')?.value || '').trim();
                        service = servicesDto.find(x => x.ServiceType === st) || null;
                    }
                }

                if (!service) {
                    service = servicesDto.find(x => x.ServiceType === aliasServiceTypeVal) || null;
                }

                // hâlâ yoksa otomatik service objesi ekle (UI’da service satırı yoksa bile kayıtta ilişki kopsun istemiyoruz)
                if (!service) {
                    const childBase = buildBaseEntityNew(nowIso);
                    service = {
                        ...childBase,
                        Id: 0,
                        GibFirmId: dto.Id || 0,
                        ServiceType: aliasServiceTypeVal,
                        StartDate: null,
                        EndDate: null,
                        TariffType: 'Kontör',
                        Status: 'Aktif',
                        Aliases: []
                    };
                    servicesDto.push(service);
                }

                const aliasBase = buildBaseEntityNew(nowIso);

                service.Aliases.push({
                    ...aliasBase,
                    Id: idVal,
                    Direction: directionVal,
                    Alias: aliasText
                });
            }

            dto.Services = servicesDto;

            console.log('[GibFirm] Gönderilen DTO:', dto);

            try {
                // 1) Önce firma kaydı
                let savedFirm;
                if (isNew) savedFirm = await FirmaApi.create(dto);
                else savedFirm = await FirmaApi.update(dto.Id, dto);

                const firmId = (savedFirm && (savedFirm.id || savedFirm.Id)) || dto.Id;

                // 2) Listeyi yenile (firma kaydı başarılıysa)
                if ($modal) $modal.modal('hide');
                await loadTable();
                bindGridRowEvents();

                // 3) Kontör hesabı (opsiyonel) - hata verirse firm kaydını BOZMASIN
                if (isNew && firmId && credits > 0) {
                    try {
                        const creditBase = buildBaseEntityNew(nowIso);
                        await GibUserCreditAccountApi.create({
                            ...creditBase,
                            Id: 0,
                            GibFirmId: firmId,
                            TotalCredits: credits,
                            UsedCredits: 0
                        });
                    } catch (creditErr) {
                        console.error('Kontör hesabı oluşturulamadı:', creditErr);
                        alert('Firma kaydedildi; ancak kontör hesabı oluşturulamadı: ' + (creditErr?.message || 'Bilinmeyen hata'));
                    }
                }
            } catch (err) {
                console.error('Firma kaydedilemedi:', err);
                alert(err.message || 'Firma kaydedilemedi (API Error).');
            } finally {
                isSaving = false;
                if (btnSave) btnSave.disabled = false;
            }
        });
    }

    // ilk yükleme
    (async () => {
        await loadTable();
        bindGridRowEvents();
    })();
}

/* DOM ready */
if (!window.__gibFirmInitializedRun) {
    window.__gibFirmInitializedRun = true;
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initGibFirm);
    } else {
        initGibFirm();
    }
}
