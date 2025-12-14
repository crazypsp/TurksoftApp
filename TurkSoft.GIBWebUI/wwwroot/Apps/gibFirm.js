import { FirmaApi, GibUserCreditAccountApi } from '../Entites/index.js';

/* =========================================================
   GLOBAL INIT GUARD (dosya cache-bust ile 2 kere yüklenirse)
   ========================================================= */
const __GIB_FIRM_KEY__ = '__gibFirmModule_v20251214';
if (window[__GIB_FIRM_KEY__]) {
    console.warn('[GibFirm] module already loaded, skipping re-init.');
} else {
    window[__GIB_FIRM_KEY__] = true;

    /* ===========================================
       Current UserId resolver (senin örneğine göre)
       =========================================== */
    function getCurrentUserIdForGib() {
        let userId = 0;

        // 1) Hidden input: <input type="hidden" id="hdnUserId" value="..." />
        const hidden = document.getElementById('hdnUserId');
        if (hidden && hidden.value) {
            const parsed = parseInt(hidden.value, 10);
            if (!isNaN(parsed) && parsed > 0) userId = parsed;
        }

        // 2) window.currentUserId
        if (!userId && typeof window.currentUserId === "number" && window.currentUserId > 0) {
            userId = window.currentUserId;
        }

        // 3) sessionStorage
        if (!userId && typeof sessionStorage !== "undefined") {
            try {
                const stored =
                    sessionStorage.getItem("CurrentUserId") ||
                    sessionStorage.getItem("currentUserId") ||
                    sessionStorage.getItem("UserId");

                if (stored) {
                    const parsed = parseInt(stored, 10);
                    if (!isNaN(parsed) && parsed > 0) userId = parsed;
                }
            } catch (e) {
                console.warn("[GibFirm] CurrentUserId sessionStorage'dan okunamadı:", e);
            }
        }

        if (!userId) {
            throw new Error("Kullanıcı Id (userId) bulunamadı. #hdnUserId / window.currentUserId / sessionStorage kontrol edin.");
        }

        return userId;
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

        // API byte[] döndürüyorsa (array) => base64'e çeviremeyiz; default kullan
        if (Array.isArray(rv)) return DEFAULT_ROWVERSION_BASE64;

        if (rv && /^0x/i.test(rv)) rv = rowVersionHexToBase64(rv);
        return (rv && String(rv).trim()) ? String(rv).trim() : DEFAULT_ROWVERSION_BASE64;
    }

    /* ===========================================
       BaseEntity builder (Firm + Child)
       =========================================== */
    function buildBaseEntityNew(nowIso, userId) {
        const uid = Number(userId || 0);
        return {
            UserId: uid,
            IsActive: true,
            DeleteDate: null,
            DeletedByUserId: null,
            CreatedAt: nowIso,
            UpdatedAt: null,
            CreatedByUserId: uid,
            UpdatedByUserId: uid,
            RowVersion: DEFAULT_ROWVERSION_BASE64
        };
    }

    function buildBaseEntityUpdate(nowIso, existing, overrideRowVersion, userId) {
        const ex = existing || {};
        const uid = Number(userId || 0);

        const createdAt = ex.createdAt || ex.CreatedAt || nowIso;

        const isActive =
            (ex.isActive !== undefined ? ex.isActive :
                (ex.IsActive !== undefined ? ex.IsActive : true));

        const deleteDate = ex.deleteDate || ex.DeleteDate || null;
        const deletedByUserId = ex.deletedByUserId || ex.DeletedByUserId || null;

        const existingUserId = ex.userId || ex.UserId || 0;
        const finalUserId = existingUserId > 0 ? existingUserId : uid;

        const existingCreatedBy = ex.createdByUserId || ex.CreatedByUserId || 0;
        const finalCreatedBy = existingCreatedBy > 0 ? existingCreatedBy : uid;

        const finalUpdatedBy = uid;

        const rowVersionBase64 = (overrideRowVersion && String(overrideRowVersion).trim())
            ? String(overrideRowVersion).trim()
            : getRowVersionFromEntityOrDefault(ex);

        return {
            UserId: finalUserId,
            IsActive: isActive,
            DeleteDate: deleteDate,
            DeletedByUserId: deletedByUserId,
            CreatedAt: createdAt,
            UpdatedAt: nowIso,
            CreatedByUserId: finalCreatedBy,
            UpdatedByUserId: finalUpdatedBy,
            RowVersion: rowVersionBase64
        };
    }

    /* ===========================================
       Hizmet / Alias sabitleri
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
        if (window.__gibFirmInitOnceDone) {
            console.warn('[GibFirm] init already done.');
            return;
        }
        window.__gibFirmInitOnceDone = true;

        console.log('[GibFirm] init');

        let currentUserId = 0;
        try {
            currentUserId = getCurrentUserIdForGib();
            console.log('[GibFirm] currentUserId:', currentUserId);
        } catch (e) {
            console.error(e);
            alert(e.message || 'Kullanıcı Id bulunamadı.');
            // userId yoksa kayıt yaptırmayalım ama ekran çalışsın
        }

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

        function nextServiceRowIndex() { serviceRowSeq += 1; return serviceRowSeq; }
        function nextAliasRowIndex() { aliasRowSeq += 1; return aliasRowSeq; }

        function val(el) { return String(el?.value || '').trim(); }

        function clearServiceAliasTables() {
            serviceRowSeq = 0;
            aliasRowSeq = 0;
            selectedServiceRowIndex = null;

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

        // Alias satırları kaybolmaz: seçili service satırı dışındakiler soluk
        function refreshAliasVisibility() {
            const aliasBody = getAliasBody();
            if (!aliasBody) return;

            const selected = getSelectedServiceRow();
            const idx = selected ? String(selected.dataset.serviceRowIndex) : null;

            aliasBody.querySelectorAll('tr').forEach(tr => {
                tr.style.display = '';
                tr.style.opacity = (!idx || String(tr.dataset.serviceRowIndex) === idx) ? '1' : '0.35';
            });
        }

        function findFirstServiceRowIndexByType(serviceType) {
            const svcBody = getSvcBody();
            if (!svcBody) return null;

            const rows = Array.from(svcBody.querySelectorAll('tr'));
            const found = rows.find(r => String(r.querySelector('.svc-serviceType')?.value || '') === String(serviceType));
            return found ? String(found.dataset.serviceRowIndex) : null;
        }

        function ensureServiceRowForType(serviceType, makeActive = false) {
            let idx = findFirstServiceRowIndexByType(serviceType);
            if (idx) return idx;

            addServiceRow({ ServiceType: serviceType, TariffType: 'Kontör', Status: 'Aktif' }, { makeActive });
            idx = findFirstServiceRowIndexByType(serviceType);
            return idx;
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

            if (!tariffType) tariffType = 'Kontör';
            if (!status) status = 'Aktif';

            const rowVersion = getRowVersionFromEntityOrDefault(service);

            const tr = document.createElement('tr');
            tr.dataset.serviceRowIndex = String(rowIndex);
            tr.innerHTML = `
                <td>
                    <input type="hidden" class="svc-id" value="${idVal}" />
                    <input type="hidden" class="svc-rowVersion" value="${rowVersion}" />
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

            const rowVersion = getRowVersionFromEntityOrDefault(alias);

            const tr = document.createElement('tr');
            tr.dataset.aliasRowIndex = String(rowIndex);
            tr.dataset.serviceRowIndex = String(serviceRowIndex);

            tr.innerHTML = `
                <td>
                    ${buildAliasServiceTypeSelect(svcType)}
                </td>
                <td>
                    <input type="hidden" class="alias-id" value="${idVal}" />
                    <input type="hidden" class="alias-rowVersion" value="${rowVersion}" />
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

            const services = (firm && (firm.services || firm.Services)) || [];
            if (Array.isArray(services)) {
                services.forEach(s => addServiceRow(s, { makeActive: false }));
            }

            if (Array.isArray(services)) {
                services.forEach(s => {
                    const svcType = s.serviceType || s.ServiceType || 'EFATURA_EARSIV';
                    const aliases = (s.aliases || s.Aliases) || [];
                    if (!Array.isArray(aliases) || !aliases.length) return;

                    // UI bağlama: aynı ServiceType’a ait ilk satıra bağla
                    const idx = ensureServiceRowForType(String(svcType), false);
                    aliases.forEach(a => addAliasRow(a, idx, svcType));
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
        function renderTable(data) {
            if (!gridBody) return;
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
        }

        async function loadTable() {
            if (!gridBody) return;
            try {
                const data = await FirmaApi.list();
                renderTable(data);
            } catch (err) {
                console.error('Firma listesi yüklenemedi:', err);
                alert(err.message || 'Firma listesi yüklenemedi.');
            }
        }

        function clearForm() {
            currentFirmEntity = null;

            if (fmId) fmId.value = '';
            if (fmRowVersion) fmRowVersion.value = DEFAULT_ROWVERSION_BASE64;

            const clear = (el, v = '') => { if (el) el.value = v; };

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

        function requireFields(list) {
            const missing = [];
            for (const item of list) {
                const v = val(item.el);
                if (!v) missing.push(item.name);
            }
            return missing;
        }

        /* ========= Delegated Events (tek sefer bağlanır) ========= */

        // Yeni Firma / Hizmet Ekle / Alias Ekle
        document.addEventListener('click', async (e) => {
            if (e.target.closest('#btnNewFirm')) {
                clearForm();
                const titleEl = document.getElementById('gibFirmModalLabel');
                if (titleEl) titleEl.textContent = 'Yeni Firma';
                if ($modal) $modal.modal('show');
                return;
            }

            if (e.target.closest('#btnNewFirmService')) {
                addServiceRow(null, { makeActive: true });
                return;
            }

            if (e.target.closest('#btnNewFirmServiceAlias')) {
                const selected = getSelectedServiceRow();
                if (!selected) { alert('Önce yukarıdaki listeden bir hizmet satırı seçin.'); return; }
                const idx = selected.dataset.serviceRowIndex;
                const svcTypeVal = selected.querySelector('.svc-serviceType')?.value || 'EFATURA_EARSIV';
                addAliasRow(null, idx, svcTypeVal);
                return;
            }

            // Firma grid edit/delete (delegation)
            if (e.target.closest('.act-edit-firm')) {
                const tr = e.target.closest('tr');
                const id = Number(tr?.dataset.id || 0);
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
                return;
            }

            if (e.target.closest('.act-del-firm')) {
                const tr = e.target.closest('tr');
                const id = Number(tr?.dataset.id || 0);
                if (!id) return;
                if (!confirm('Bu firma silinsin mi?')) return;

                try {
                    await FirmaApi.remove(id);
                    await loadTable();
                } catch (err) {
                    console.error('Firma silinemedi:', err);
                    alert(err.message || 'Firma silinemedi.');
                }
                return;
            }

            // Hizmet grid içi: satır seç / sil
            const svcBody = getSvcBody();
            if (svcBody && svcBody.contains(e.target)) {
                const tr = e.target.closest('tr');
                if (!tr) return;

                if (e.target.closest('.act-del-service')) {
                    const idx = tr.dataset.serviceRowIndex;
                    tr.remove();

                    // Bu service row’a bağlı aliasları silmek istemiyorsan aşağıyı yorumla:
                    const aliasBody = getAliasBody();
                    if (aliasBody) {
                        Array.from(aliasBody.querySelectorAll('tr')).forEach(r => {
                            if (String(r.dataset.serviceRowIndex) === String(idx)) r.remove();
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
                    return;
                }

                svcBody.querySelectorAll('tr').forEach(r => r.classList.remove('active'));
                tr.classList.add('active');
                selectedServiceRowIndex = tr.dataset.serviceRowIndex;
                refreshAliasVisibility();
                return;
            }

            // Alias grid içi: sil
            const aliasBody = getAliasBody();
            if (aliasBody && aliasBody.contains(e.target)) {
                if (e.target.closest('.act-del-alias')) {
                    e.target.closest('tr')?.remove();
                    refreshAliasVisibility();
                    return;
                }
            }
        });

        // ServiceType / Alias ServiceType değişimleri
        document.addEventListener('change', (e) => {
            // Service row type değişince: o rowIndex’e bağlı alias satırlarının serviceType’ını güncelle
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

            // Alias row type değişince: UI bağını o tipe ait ilk service row’a al (yoksa service aç)
            const aliasSel = e.target.closest('.alias-serviceType');
            if (aliasSel) {
                const aRow = aliasSel.closest('tr');
                if (!aRow) return;

                const newType = String(aliasSel.value);
                let targetRowIndex = ensureServiceRowForType(newType, false);
                if (targetRowIndex) {
                    aRow.dataset.serviceRowIndex = String(targetRowIndex);
                }
                refreshAliasVisibility();
            }
        });

        /* ========= Submit ========= */
        let isSaving = false;

        if (form) {
            form.addEventListener('submit', async (e) => {
                e.preventDefault();
                if (isSaving) return;

                // userId olmadan kayıt olmaz
                if (!currentUserId) {
                    alert('Kullanıcı Id bulunamadı. Oturumu kontrol edin.');
                    return;
                }

                if (!fmCommercialRegistrationNo || !fmMersisNo) {
                    alert('CSHTML’de CommercialRegistrationNo ve MersisNo inputları yok. Ekleyip tekrar deneyin.');
                    return;
                }

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
                    ? buildBaseEntityNew(nowIso, currentUserId)
                    : buildBaseEntityUpdate(nowIso, currentFirmEntity, overrideRv, currentUserId);

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

                // ---------- Services + Aliases (NESTED) ----------
                // Kaydetme tarafında ilişkiyi ServiceType üzerinden kuruyoruz (UI rowIndex sadece görsel)
                const servicesByType = new Map(); // ServiceType -> serviceDto

                const svcBody = getSvcBody();
                const aliasBody = getAliasBody();

                const serviceRows = svcBody ? Array.from(svcBody.querySelectorAll('tr')) : [];
                for (const tr of serviceRows) {
                    const serviceTypeVal = String(tr.querySelector('.svc-serviceType')?.value || '').trim();
                    if (!serviceTypeVal) continue;

                    const idVal = parseInt((tr.querySelector('.svc-id')?.value || '0'), 10) || 0;
                    const rv = String(tr.querySelector('.svc-rowVersion')?.value || '').trim() || DEFAULT_ROWVERSION_BASE64;

                    const startDate = normalizeDateToIso(val(tr.querySelector('.svc-startDate')));
                    const endDate = normalizeDateToIso(val(tr.querySelector('.svc-endDate')));
                    let tariffType = val(tr.querySelector('.svc-tariffType'));
                    let status = String(tr.querySelector('.svc-status')?.value || '').trim();

                    if (!tariffType) tariffType = 'Kontör';
                    if (!status) status = 'Aktif';

                    // aynı ServiceType birden fazla satırsa: ilkini baz al, Id varsa onu tercih et
                    const existing = servicesByType.get(serviceTypeVal);
                    if (!existing) {
                        const childBase = buildBaseEntityNew(nowIso, currentUserId);
                        servicesByType.set(serviceTypeVal, {
                            ...childBase,
                            // existing kayıtsa rowVersion'ı satırdan al
                            RowVersion: rv,
                            Id: idVal,
                            GibFirmId: dto.Id || 0,
                            ServiceType: serviceTypeVal,
                            StartDate: startDate || null,
                            EndDate: endDate || null,
                            TariffType: tariffType,
                            Status: status,
                            Aliases: []
                        });
                    } else {
                        // merge: Id > 0 olanı koru
                        if (!existing.Id && idVal) existing.Id = idVal;
                        if (rv && rv !== DEFAULT_ROWVERSION_BASE64) existing.RowVersion = rv;

                        // date merge (basit)
                        if (!existing.StartDate && startDate) existing.StartDate = startDate;
                        if (!existing.EndDate && endDate) existing.EndDate = endDate;
                        if (!existing.TariffType && tariffType) existing.TariffType = tariffType;
                        if (!existing.Status && status) existing.Status = status;
                    }
                }

                const aliasRows = aliasBody ? Array.from(aliasBody.querySelectorAll('tr')) : [];
                for (const tr of aliasRows) {
                    const aliasText = val(tr.querySelector('.alias-value'));
                    if (!aliasText) continue;

                    const aliasServiceTypeVal = String(tr.querySelector('.alias-serviceType')?.value || 'EFATURA_EARSIV').trim();
                    const directionVal = String(tr.querySelector('.alias-direction')?.value || 'SENDER').trim();
                    const idVal = parseInt((tr.querySelector('.alias-id')?.value || '0'), 10) || 0;
                    const rv = String(tr.querySelector('.alias-rowVersion')?.value || '').trim() || DEFAULT_ROWVERSION_BASE64;

                    let svc = servicesByType.get(aliasServiceTypeVal);
                    if (!svc) {
                        // alias var ama service satırı yoksa otomatik service oluştur
                        const childBase = buildBaseEntityNew(nowIso, currentUserId);
                        svc = {
                            ...childBase,
                            RowVersion: DEFAULT_ROWVERSION_BASE64,
                            Id: 0,
                            GibFirmId: dto.Id || 0,
                            ServiceType: aliasServiceTypeVal,
                            StartDate: null,
                            EndDate: null,
                            TariffType: 'Kontör',
                            Status: 'Aktif',
                            Aliases: []
                        };
                        servicesByType.set(aliasServiceTypeVal, svc);
                    }

                    const aliasBase = buildBaseEntityNew(nowIso, currentUserId);
                    svc.Aliases.push({
                        ...aliasBase,
                        RowVersion: rv,
                        Id: idVal,
                        Direction: directionVal,
                        Alias: aliasText
                    });
                }

                dto.Services = Array.from(servicesByType.values());

                try {
                    let savedFirm;

                    // ✅ Duplicate TaxNo yakalayıp otomatik update denemesi (upsert)
                    if (isNew) {
                        try {
                            savedFirm = await FirmaApi.create(dto);
                        } catch (createErr) {
                            const msg = String(createErr?.message || createErr || '');
                            if (msg.includes('IX_GibFirm_UserId_TaxNo') || msg.includes('duplicate key') || msg.includes('TaxNo')) {
                                // mevcut firmayı bulup update dene
                                const list = await FirmaApi.list();
                                const found = (list || []).find(x => String(x.taxNo || x.TaxNo || '') === dto.TaxNo);
                                if (found) {
                                    const foundId = found.id || found.Id;
                                    const full = await FirmaApi.get(foundId);

                                    // rowVersion + audit doğru gelsin diye update baseEntity’yi tekrar kur
                                    const upBase = buildBaseEntityUpdate(nowIso, full, getRowVersionFromEntityOrDefault(full), currentUserId);

                                    const upDto = {
                                        ...dto,
                                        ...upBase,
                                        Id: Number(foundId)
                                    };

                                    savedFirm = await FirmaApi.update(Number(foundId), upDto);
                                } else {
                                    throw createErr;
                                }
                            } else {
                                throw createErr;
                            }
                        }
                    } else {
                        savedFirm = await FirmaApi.update(dto.Id, dto);
                    }

                    const firmId = (savedFirm && (savedFirm.id || savedFirm.Id)) || dto.Id;

                    if ($modal) $modal.modal('hide');
                    await loadTable();

                    // Kontör hesabı (opsiyonel) - firma kaydını bozmasın
                    if (isNew && firmId && credits > 0) {
                        try {
                            const creditBase = buildBaseEntityNew(nowIso, currentUserId);
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
        })();
    }

    /* DOM ready */
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initGibFirm);
    } else {
        initGibFirm();
    }
}
