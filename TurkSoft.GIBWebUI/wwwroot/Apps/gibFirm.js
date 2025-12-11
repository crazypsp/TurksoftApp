import { FirmaApi, GibUserCreditAccountApi } from '../Entites/index.js';

/* ===========================================
   RowVersion yardımcıları
   =========================================== */

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

const DEFAULT_ROWVERSION_HEX = "0x00000000000007E1";
const DEFAULT_ROWVERSION_BASE64 = rowVersionHexToBase64(DEFAULT_ROWVERSION_HEX);

function getRowVersionFromEntityOrDefault(entity) {
    if (!entity) return DEFAULT_ROWVERSION_BASE64;

    let rv =
        entity.rowVersionBase64 || entity.RowVersionBase64 ||
        entity.rowVersion || entity.RowVersion ||
        entity.rowVersionHex || entity.RowVersionHex;

    if (rv && /^0x/i.test(rv)) {
        rv = rowVersionHexToBase64(rv);
    }

    return rv || DEFAULT_ROWVERSION_BASE64;
}

/* ===========================================
   BaseEntity builder
   =========================================== */

function buildBaseEntityForFirm(isNew, nowIso, existing, overrideRowVersion) {
    const ex = existing || {};
    const BASE_USER_ID = 1;

    if (isNew) {
        return {
            UserId: BASE_USER_ID,
            IsActive: true,
            DeleteDate: null,
            DeletedByUserId: null,
            CreatedAt: nowIso,
            UpdatedAt: null,
            CreatedByUserId: BASE_USER_ID,
            UpdatedByUserId: BASE_USER_ID,
            RowVersion: overrideRowVersion || DEFAULT_ROWVERSION_BASE64
        };
    }

    const createdAt = ex.createdAt || ex.CreatedAt || nowIso;
    const isActive =
        (ex.isActive !== undefined ? ex.isActive :
            (ex.IsActive !== undefined ? ex.IsActive : true));

    const deleteDate = ex.deleteDate || ex.DeleteDate || null;
    const deletedByUserId = ex.deletedByUserId || ex.DeletedByUserId || null;

    const createdByUserId =
        ex.createdByUserId || ex.CreatedByUserId || BASE_USER_ID;

    const updatedByUserId =
        ex.updatedByUserId || ex.UpdatedByUserId || BASE_USER_ID;

    const userId =
        ex.userId || ex.UserId || BASE_USER_ID;

    let rowVersionBase64 = overrideRowVersion;
    if (!rowVersionBase64) {
        rowVersionBase64 = getRowVersionFromEntityOrDefault(ex);
    }

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
   Hizmet / Alias yardımcıları
   =========================================== */

function serviceTypeLabel(val) {
    const v = String(val || '1');
    switch (v) {
        case '1': return 'E-Fatura / E-Arşiv';
        case '2': return 'E-İrsaliye';
        case '3': return 'E-Defter';
        case '4': return 'E-MM';
        case '5': return 'E-Bilet';
        default: return 'Bilinmiyor';
    }
}

function buildServiceTypeSelect(selectedVal) {
    const v = String(selectedVal || '1');
    return `
        <select class="form-control svc-serviceType">
            <option value="1" ${v === '1' ? 'selected' : ''}>E-Fatura / E-Arşiv</option>
            <option value="2" ${v === '2' ? 'selected' : ''}>E-İrsaliye</option>
            <option value="3" ${v === '3' ? 'selected' : ''}>E-Defter</option>
            <option value="4" ${v === '4' ? 'selected' : ''}>E-MM</option>
            <option value="5" ${v === '5' ? 'selected' : ''}>E-Bilet</option>
        </select>`;
}

/* ===========================================
   MAIN INIT
   =========================================== */

function initGibFirm() {
    console.log('[GibFirm] init çalıştı');

    const gridBody = document.querySelector('#tblGibFirms tbody');
    const form = document.getElementById('gibFirmForm');
    const $modal = window.$ ? window.$('#gibFirmModal') : null;

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

    // ------- Hizmet / Alias tablo erişimleri dinamik olsun -------
    function getSvcBody() {
        return document.querySelector('#FirmServiceGrid tbody');
    }

    function getAliasBody() {
        return document.querySelector('#FirmServiceAliasGrid tbody');
    }

    // ------- State -------

    let currentFirmEntity = null;

    let serviceRowSeq = 0;
    let aliasRowSeq = 0;
    let selectedServiceRowIndex = null;
    const serviceRowIndexByServiceId = {};

    function nextServiceRowIndex() { serviceRowSeq += 1; return serviceRowSeq; }
    function nextAliasRowIndex() { aliasRowSeq += 1; return aliasRowSeq; }

    function clearServiceAliasTables() {
        serviceRowSeq = 0;
        aliasRowSeq = 0;
        selectedServiceRowIndex = null;
        for (const k in serviceRowIndexByServiceId) {
            if (Object.prototype.hasOwnProperty.call(serviceRowIndexByServiceId, k)) {
                delete serviceRowIndexByServiceId[k];
            }
        }
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

    function refreshAliasVisibility() {
        const aliasBody = getAliasBody();
        const svcBody = getSvcBody();
        if (!aliasBody || !svcBody) return;

        const selected = getSelectedServiceRow();
        if (!selected) {
            aliasBody.querySelectorAll('tr').forEach(tr => tr.style.display = 'none');
            return;
        }
        const idx = selected.dataset.serviceRowIndex;
        aliasBody.querySelectorAll('tr').forEach(tr => {
            tr.style.display = (String(tr.dataset.serviceRowIndex) === String(idx)) ? '' : 'none';
        });
    }

    function addServiceRow(service) {
        console.log('[GibFirm] addServiceRow', service);
        const svcBody = getSvcBody();
        if (!svcBody) {
            console.warn('[GibFirm] FirmServiceGrid tbody bulunamadı');
            return;
        }

        const rowIndex = nextServiceRowIndex();
        const idVal = service ? (service.id || service.Id || 0) : 0;
        const serviceTypeVal = service ? (service.serviceType || service.ServiceType || 1) : 1;

        let startDate = service ? (service.startDate || service.StartDate || '') : '';
        let endDate = service ? (service.endDate || service.EndDate || '') : '';
        const tariffType = service ? (service.tariffType || service.TariffType || '') : '';
        let status = service ? (service.status || service.Status || '') : '';

        if (startDate && typeof startDate === 'string' && startDate.length >= 10) startDate = startDate.substring(0, 10);
        if (endDate && typeof endDate === 'string' && endDate.length >= 10) endDate = endDate.substring(0, 10);
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

        svcBody.querySelectorAll('tr').forEach(r => r.classList.remove('active'));
        tr.classList.add('active');
        selectedServiceRowIndex = rowIndex;
        refreshAliasVisibility();
    }

    function getServiceTypeValueByRowIndex(idx) {
        const svcBody = getSvcBody();
        if (!svcBody) return '1';
        const tr = Array.from(svcBody.querySelectorAll('tr'))
            .find(r => String(r.dataset.serviceRowIndex) === String(idx));
        if (!tr) return '1';
        const sel = tr.querySelector('.svc-serviceType');
        return sel ? (sel.value || '1') : '1';
    }

    function addAliasRow(alias, serviceRowIndex, serviceTypeVal) {
        console.log('[GibFirm] addAliasRow', alias, serviceRowIndex);
        const aliasBody = getAliasBody();
        if (!aliasBody) {
            console.warn('[GibFirm] FirmServiceAliasGrid tbody bulunamadı');
            return;
        }

        if (!serviceRowIndex) {
            const sel = getSelectedServiceRow();
            if (!sel) return;
            serviceRowIndex = sel.dataset.serviceRowIndex;
        }

        const rowIndex = nextAliasRowIndex();
        const idVal = alias ? (alias.id || alias.Id || 0) : 0;

        const directionVal = alias ? (alias.direction || alias.Direction || 1) : 1;
        const aliasText = alias ? (alias.alias || alias.Alias || '') : '';

        const svcType = serviceTypeVal ||
            (alias ? (alias.serviceType || alias.ServiceType || 1) :
                getServiceTypeValueByRowIndex(serviceRowIndex));
        const svcLabel = serviceTypeLabel(svcType);

        const tr = document.createElement('tr');
        tr.dataset.aliasRowIndex = String(rowIndex);
        tr.dataset.serviceRowIndex = String(serviceRowIndex);
        tr.innerHTML = `
            <td class="alias-serviceType-text">${svcLabel}</td>
            <td>
                <input type="hidden" class="alias-id" value="${idVal}" />
                <select class="form-control alias-direction">
                    <option value="1" ${String(directionVal) === '1' ? 'selected' : ''}>Gönderici</option>
                    <option value="2" ${String(directionVal) === '2' ? 'selected' : ''}>Alıcı</option>
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
        const svcBody = getSvcBody();
        const aliasBody = getAliasBody();
        if (!svcBody || !aliasBody) return;

        clearServiceAliasTables();

        const services = (firm && (firm.services || firm.Services)) || [];
        if (Array.isArray(services)) services.forEach(s => addServiceRow(s));

        const aliases = (firm && (firm.aliases || firm.Aliases)) || [];
        if (Array.isArray(aliases)) {
            aliases.forEach(a => {
                const sid =
                    a.gibFirmServiceId || a.GibFirmServiceId ||
                    a.serviceId || a.ServiceId || 0;
                const rowIdx = sid ? serviceRowIndexByServiceId[sid] : null;
                const svcType = a.serviceType || a.ServiceType || 1;
                if (!rowIdx) return;
                addAliasRow(a, rowIdx, svcType);
            });
        }

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
                <tr data-id="${f.id}">
                    <td>${f.title || f.Title || ''}</td>
                    <td>${f.taxNo || f.TaxNo || ''}</td>
                    <td>${f.gibAlias || f.GibAlias || ''}</td>
                    <td>${(f.isEInvoiceRegistered || f.IsEInvoiceRegistered) ? 'Evet' : 'Hayır'}</td>
                    <td>${(f.isEArchiveRegistered || f.IsEArchiveRegistered) ? 'Evet' : 'Hayır'}</td>
                    <td class="text-center">
                        <button class="btn btn-warning btn-sm act-edit-firm" type="button">
                            <i class="fa fa-edit"></i>
                        </button>
                        <button class="btn btn-danger btn-sm act-del-firm" type="button">
                            <i class="fa fa-trash"></i>
                        </button>
                    </td>
                </tr>
            `).join('');
            bindGridRowEvents();
        } catch (err) {
            console.error('Firma listesi yüklenemedi:', err);
            alert(err.message || 'Firma listesi yüklenemedi.');
        }
    }

    function clearForm() {
        currentFirmEntity = null;

        if (fmId) fmId.value = '';
        if (fmRowVersion) fmRowVersion.value = DEFAULT_ROWVERSION_BASE64;

        if (fmTaxNo) fmTaxNo.value = '';
        if (fmCustomerName) fmCustomerName.value = '';
        if (fmTitle) fmTitle.value = '';
        if (fmKepAddress) fmKepAddress.value = '';
        if (fmPersonalFirstName) fmPersonalFirstName.value = '';
        if (fmInstitutionType) fmInstitutionType.value = '1';
        if (fmPersonalLastName) fmPersonalLastName.value = '';
        if (fmCorporateEmail) fmCorporateEmail.value = '';
        if (fmTaxOfficeProvince) fmTaxOfficeProvince.value = '';
        if (fmTaxOffice) fmTaxOffice.value = '';
        if (fmCustomerRepresentative) fmCustomerRepresentative.value = '';

        if (fmResponsibleTckn) fmResponsibleTckn.value = '';
        if (fmResponsibleFirstName) fmResponsibleFirstName.value = '';
        if (fmResponsibleLastName) fmResponsibleLastName.value = '';
        if (fmResponsibleMobilePhone) fmResponsibleMobilePhone.value = '';
        if (fmResponsibleEmail) fmResponsibleEmail.value = '';

        if (fmCreatedByPersonFirstName) fmCreatedByPersonFirstName.value = '';
        if (fmCreatedByPersonLastName) fmCreatedByPersonLastName.value = '';
        if (fmCreatedByPersonMobilePhone) fmCreatedByPersonMobilePhone.value = '';

        if (fmAddress) fmAddress.value = '';
        if (fmCity) fmCity.value = '';
        if (fmDistrict) fmDistrict.value = '';
        if (fmCountry) fmCountry.value = '';
        if (fmPostalCode) fmPostalCode.value = '';
        if (fmPhone) fmPhone.value = '';
        if (fmEmail) fmEmail.value = '';

        if (fmGibAlias) fmGibAlias.value = '';
        if (fmApiKey) fmApiKey.value = '';
        if (fmIsEInv) fmIsEInv.checked = false;
        if (fmIsEArch) fmIsEArch.checked = false;
        if (fmInitialCredits) fmInitialCredits.value = '';

        clearServiceAliasTables();
    }

    function bindGridRowEvents() {
        if (!gridBody) return;

        gridBody.querySelectorAll('.act-edit-firm').forEach(btn => {
            btn.addEventListener('click', async () => {
                const tr = btn.closest('tr');
                const id = tr ? Number(tr.dataset.id) : 0;
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

                    if (fmInitialCredits) fmInitialCredits.value = '';

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
                const tr = btn.closest('tr');
                const id = tr ? Number(tr.dataset.id) : 0;
                if (!id) return;
                if (!confirm('Bu firma silinsin mi?')) return;
                try {
                    await FirmaApi.remove(id);
                    await loadTable();
                } catch (err) {
                    console.error('Firma silinemedi:', err);
                    alert(err.message || 'Firma silinemedi.');
                }
            });
        });
    }

    // ----- Hizmet / Alias buton click handler'ları -----

    function onAddServiceClick(ev) {
        if (ev && ev.preventDefault) ev.preventDefault();
        addServiceRow(null);
    }

    function onAddAliasClick(ev) {
        if (ev && ev.preventDefault) ev.preventDefault();
        const selected = getSelectedServiceRow();
        if (!selected) {
            alert('Önce yukarıdaki listeden bir hizmet satırı seçin.');
            return;
        }
        const idx = selected.dataset.serviceRowIndex;
        const sel = selected.querySelector('.svc-serviceType');
        const svcTypeVal = sel ? (sel.value || '1') : '1';
        addAliasRow(null, idx, svcTypeVal);
    }

    // Eski inline onclick'ler varsa hata vermesin diye:
    window.__gib_addServiceRow = onAddServiceClick;
    window.__gib_addServiceAlias = onAddAliasClick;

    // ----- Document seviyesinde delegasyon (buton + satırlar) -----

    document.addEventListener('click', function (e) {
        // Yeni Firma
        const btnNewFirm = e.target.closest('#btnNewFirm');
        if (btnNewFirm) {
            clearForm();
            const titleEl = document.getElementById('gibFirmModalLabel');
            if (titleEl) titleEl.textContent = 'Yeni Firma';
            if ($modal) $modal.modal('show');
            return;
        }

        // Hizmet ekle
        const btnSvc = e.target.closest('#btnNewFirmService');
        if (btnSvc) {
            onAddServiceClick(e);
            return;
        }

        // Alias ekle
        const btnAlias = e.target.closest('#btnNewFirmServiceAlias');
        if (btnAlias) {
            onAddAliasClick(e);
            return;
        }

        // --------- Hizmet satırları ---------
        const svcBody = getSvcBody();
        if (svcBody && svcBody.contains(e.target)) {
            const tr = e.target.closest('tr');
            if (!tr) return;

            const delBtn = e.target.closest('.act-del-service');
            if (delBtn) {
                const idx = tr.dataset.serviceRowIndex;
                tr.remove();

                const aliasBody = getAliasBody();
                if (aliasBody) {
                    Array.from(aliasBody.querySelectorAll('tr')).forEach(r => {
                        if (String(r.dataset.serviceRowIndex) === String(idx)) {
                            r.remove();
                        }
                    });
                }
                const firstRow = svcBody.querySelector('tr');
                svcBody.querySelectorAll('tr').forEach(r => r.classList.remove('active'));
                if (firstRow) {
                    firstRow.classList.add('active');
                    selectedServiceRowIndex = firstRow.dataset.serviceRowIndex;
                } else {
                    selectedServiceRowIndex = null;
                }
                refreshAliasVisibility();
            } else {
                svcBody.querySelectorAll('tr').forEach(r => r.classList.remove('active'));
                tr.classList.add('active');
                selectedServiceRowIndex = tr.dataset.serviceRowIndex;
                refreshAliasVisibility();
            }
        }

        // --------- Alias satırları (silme) ---------
        const aliasBody = getAliasBody();
        if (aliasBody && aliasBody.contains(e.target)) {
            const delAliasBtn = e.target.closest('.act-del-alias');
            if (delAliasBtn) {
                const tr = delAliasBtn.closest('tr');
                if (tr) tr.remove();
                refreshAliasVisibility();
            }
        }
    });

    /* ========= Form submit ========= */

    if (form) {
        form.addEventListener('submit', async (e) => {
            e.preventDefault();

            const isNew = !fmId || !fmId.value;
            const nowIso = new Date().toISOString();

            if (fmTitle && !fmTitle.value) {
                alert('Ünvan zorunludur.');
                return;
            }

            const creditsRaw = fmInitialCredits ? (fmInitialCredits.value || '').trim() : '';
            const credits = creditsRaw ? parseInt(creditsRaw, 10) : 0;
            if (creditsRaw && (isNaN(credits) || credits < 0)) {
                alert('Başlangıç kontör değeri geçersiz.');
                return;
            }

            const overrideRv = fmRowVersion && fmRowVersion.value
                ? fmRowVersion.value.trim()
                : null;

            const baseEntity = buildBaseEntityForFirm(
                isNew,
                nowIso,
                isNew ? null : currentFirmEntity,
                overrideRv
            );

            const dto = {
                ...baseEntity,
                Id: isNew ? 0 : Number(fmId.value),

                TaxNo: fmTaxNo ? fmTaxNo.value : '',
                CustomerName: fmCustomerName ? fmCustomerName.value : '',
                Title: fmTitle ? fmTitle.value : '',
                KepAddress: fmKepAddress ? fmKepAddress.value : '',
                PersonalFirstName: fmPersonalFirstName ? fmPersonalFirstName.value : '',
                InstitutionType: fmInstitutionType ? parseInt(fmInstitutionType.value || '0', 10) : 0,
                PersonalLastName: fmPersonalLastName ? fmPersonalLastName.value : '',
                CorporateEmail: fmCorporateEmail ? fmCorporateEmail.value : '',
                TaxOfficeProvince: fmTaxOfficeProvince ? fmTaxOfficeProvince.value : '',
                TaxOffice: fmTaxOffice ? fmTaxOffice.value : '',
                CustomerRepresentative: fmCustomerRepresentative ? fmCustomerRepresentative.value : '',

                ResponsibleTckn: fmResponsibleTckn ? fmResponsibleTckn.value : '',
                ResponsibleFirstName: fmResponsibleFirstName ? fmResponsibleFirstName.value : '',
                ResponsibleLastName: fmResponsibleLastName ? fmResponsibleLastName.value : '',
                ResponsibleMobilePhone: fmResponsibleMobilePhone ? fmResponsibleMobilePhone.value : '',
                ResponsibleEmail: fmResponsibleEmail ? fmResponsibleEmail.value : '',

                CreatedByPersonFirstName: fmCreatedByPersonFirstName ? fmCreatedByPersonFirstName.value : '',
                CreatedByPersonLastName: fmCreatedByPersonLastName ? fmCreatedByPersonLastName.value : '',
                CreatedByPersonMobilePhone: fmCreatedByPersonMobilePhone ? fmCreatedByPersonMobilePhone.value : '',

                AddressLine: fmAddress ? fmAddress.value : '',
                City: fmCity ? fmCity.value : '',
                District: fmDistrict ? fmDistrict.value : '',
                Country: fmCountry ? fmCountry.value : '',
                PostalCode: fmPostalCode ? fmPostalCode.value : '',
                Phone: fmPhone ? fmPhone.value : '',
                Email: fmEmail ? fmEmail.value : '',

                GibAlias: fmGibAlias ? fmGibAlias.value : '',
                ApiKey: fmApiKey ? fmApiKey.value : '',
                IsEInvoiceRegistered: fmIsEInv ? fmIsEInv.checked : false,
                IsEArchiveRegistered: fmIsEArch ? fmIsEArch.checked : false
            };

            // ----- Hizmetler -----
            const servicesDto = [];
            const svcBody = getSvcBody();
            if (svcBody) {
                Array.from(svcBody.querySelectorAll('tr')).forEach(tr => {
                    const idInput = tr.querySelector('.svc-id');
                    const idVal = parseInt(idInput ? idInput.value || '0' : '0', 10) || 0;
                    const selType = tr.querySelector('.svc-serviceType');
                    const serviceTypeVal = selType ? selType.value : '';
                    const startDate = (tr.querySelector('.svc-startDate')?.value || '').trim();
                    const endDate = (tr.querySelector('.svc-endDate')?.value || '').trim();
                    const tariffType = (tr.querySelector('.svc-tariffType')?.value || '').trim();
                    const statusSel = tr.querySelector('.svc-status');
                    const status = statusSel ? statusSel.value : '';

                    if (!serviceTypeVal && !startDate && !endDate && !tariffType && !status) return;

                    servicesDto.push({
                        Id: idVal,
                        GibFirmId: dto.Id || 0,
                        ServiceType: serviceTypeVal ? parseInt(serviceTypeVal, 10) : 0,
                        StartDate: startDate || null,
                        EndDate: endDate || null,
                        TariffType: tariffType || null,
                        Status: status || null
                    });
                });
            }

            // ----- Aliaslar -----
            const aliasesDto = [];
            const aliasBody = getAliasBody();
            if (aliasBody && svcBody) {
                Array.from(aliasBody.querySelectorAll('tr')).forEach(tr => {
                    const idInput = tr.querySelector('.alias-id');
                    const idVal = parseInt(idInput ? idInput.value || '0' : '0', 10) || 0;
                    const aliasValueInput = tr.querySelector('.alias-value');
                    const aliasText = aliasValueInput ? (aliasValueInput.value || '').trim() : '';
                    if (!aliasText) return;

                    const dirSelect = tr.querySelector('.alias-direction');
                    const directionVal = dirSelect ? dirSelect.value || '1' : '1';

                    const svcRowIndex = tr.dataset.serviceRowIndex;
                    const svcRow = Array.from(svcBody.querySelectorAll('tr'))
                        .find(r => String(r.dataset.serviceRowIndex) === String(svcRowIndex));

                    let serviceTypeVal = '1';
                    let svcIdVal = 0;
                    if (svcRow) {
                        const stSel = svcRow.querySelector('.svc-serviceType');
                        serviceTypeVal = stSel ? stSel.value || '1' : '1';
                        const idInp = svcRow.querySelector('.svc-id');
                        svcIdVal = parseInt(idInp ? idInp.value || '0' : '0', 10) || 0;
                    }

                    aliasesDto.push({
                        Id: idVal,
                        GibFirmServiceId: svcIdVal || 0,
                        ServiceType: serviceTypeVal ? parseInt(serviceTypeVal, 10) : 0,
                        Direction: directionVal ? parseInt(directionVal, 10) : 0,
                        Alias: aliasText
                    });
                });
            }

            dto.Services = servicesDto;
            dto.Aliases = aliasesDto;

            console.log('[GibFirm] Gönderilen DTO:', dto);

            try {
                let savedFirm;
                if (isNew) {
                    savedFirm = await FirmaApi.create(dto);
                } else {
                    savedFirm = await FirmaApi.update(dto.Id, dto);
                }

                const firmId =
                    (savedFirm && (savedFirm.id || savedFirm.Id)) ||
                    dto.Id;

                if (isNew && firmId && credits > 0) {
                    const creditBase = buildBaseEntityForFirm(true, nowIso, null, null);
                    await GibUserCreditAccountApi.create({
                        ...creditBase,
                        Id: 0,
                        GibFirmId: firmId,
                        TotalCredits: credits,
                        UsedCredits: 0
                    });
                }

                if ($modal) $modal.modal('hide');
                await loadTable();
            } catch (err) {
                console.error('Firma kaydedilemedi:', err);
                alert(err.message || 'Firma kaydedilemedi (API Error 500).');
            }
        });
    }

    loadTable();
}

/* DOM ready */
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initGibFirm);
} else {
    initGibFirm();
}
