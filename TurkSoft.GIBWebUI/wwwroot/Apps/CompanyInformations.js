// wwwroot/js/pages/Settings/CompanyInformations.js
import { CompanyInformationsApi, ContractApi, BranchApi } from '../Entities/index.js';

const $$ = sel => $(sel).toArray();

/* ============================================================
   RowVersion yardımcıları (hex -> base64 + entity'den okuma)
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
   BaseEntity yardımcıları (UserId, CreatedByUserId, RowVersion)
   ============================================================ */

function getCurrentUserId() {
    return window.currentUserId || window.appUserId || 1;
}

function buildBaseForCreate() {
    const userId = getCurrentUserId();
    return {
        UserId: userId,
        CreatedByUserId: userId,
        UpdatedByUserId: userId,
        IsActive: true,
        RowVersion: ""
    };
}

function buildBaseForUpdate(entity) {
    const userId = getCurrentUserId();
    const rvB64 = getRowVersionB64FromEntity(entity || {});
    const isActive =
        (entity && entity.isActive !== undefined)
            ? entity.isActive
            : (entity && entity.IsActive !== undefined)
                ? entity.IsActive
                : true;

    return {
        UserId: (entity && (entity.userId || entity.UserId)) || userId,
        CreatedByUserId: (entity && (entity.createdByUserId || entity.CreatedByUserId)) || userId,
        UpdatedByUserId: userId,
        IsActive: isActive,
        RowVersion: rvB64
    };
}

/* ============================================================
   Tarih yardımcıları
   ============================================================ */

function toStr(v) {
    return (v === undefined || v === null) ? '' : String(v);
}

function formatDateDisplay(val) {
    if (!val) return '';
    try {
        if (window.moment) {
            return window.moment(val).format('DD.MM.YYYY');
        }
        // basit fallback
        const d = new Date(val);
        if (isNaN(d.getTime())) return toStr(val);
        const dd = String(d.getDate()).padStart(2, '0');
        const mm = String(d.getMonth() + 1).padStart(2, '0');
        const yyyy = d.getFullYear();
        return `${dd}.${mm}.${yyyy}`;
    } catch (e) {
        return toStr(val);
    }
}

function parseDateInput(val) {
    const s = (val || '').trim();
    if (!s) return null;

    // "DD.MM.YYYY" formatı
    const parts = s.split('.');
    if (parts.length === 3) {
        const d = parseInt(parts[0], 10);
        const m = parseInt(parts[1], 10);
        const y = parseInt(parts[2], 10);
        if (!isNaN(d) && !isNaN(m) && !isNaN(y)) {
            const jsDate = new Date(y, m - 1, d);
            if (!isNaN(jsDate.getTime())) {
                return jsDate.toISOString();
            }
        }
    }

    // Diğer her şey için Date'e bırak
    const d2 = new Date(s);
    if (!isNaN(d2.getTime())) {
        return d2.toISOString();
    }

    return null;
}

/* ============================================================
   MAIN
   ============================================================ */

$(document).ready(function () {

    /* ----------------------------------------------------
       Firma Form Elementleri
       ---------------------------------------------------- */
    const formEl = $('#frmCompany'); // cshtml'de form id farklıysa bunu değiştir

    const fmId = $('#cmpId');           // hidden Id
    const fmVkn = $('#vknTckn');        // VKN/TCKN
    const fmName = $('#musteriAdi');    // Müşteri Adı
    const fmTitle = $('#unvan');        // Unvan
    const fmKep = $('#kepAdresi');      // Kep Adresi

    const fmPersonName = $('#ad');      // Ad (*Tckn)
    const fmPersonSurname = $('#soyad');// Soyad (*Tckn)
    const fmKurumTuru = $('#kurumTuru');// Kurum Türü
    const fmCorporateMail = $('#kurumsalEposta'); // Kurumsal e-Posta

    const fmTaxOfficeCity = $('#vergiDairesiIl');  // Vergi Dairesi İl
    const fmTaxOffice = $('#vergiDairesi');        // Vergi Dairesi
    const fmCustomerRep = $('#musteriTemsilcisi'); // Müşteri Temsilcisi

    const fmRespTckn = $('#sorumluTckn');          // Sorumlu Tckn
    const fmRespAd = $('#sorumluAd');              // Sorumlu Ad
    const fmRespSoyad = $('#sorumluSoyad');        // Sorumlu Soyad
    const fmRespPhone = $('#sorumluCepTel');       // Sorumlu Cep Tel
    const fmRespEmail = $('#sorumluEPosta');       // Sorumlu e-Posta

    const fmRegName = $('#kaydedenAd');            // Kaydeden Ad
    const fmRegSurname = $('#kaydedenSoyad');      // Kaydeden Soyad
    const fmRegPhone = $('#kaydedenTel');          // Kaydeden Cep No

    /* ----------------------------------------------------
       Grid Elementleri
       ---------------------------------------------------- */
    const gridContracts = $('#SozlesmeGrid tbody');
    const gridBranches = $('#SubeGrid tbody');

    let currentCompanyId = null;

    /* ----------------------------------------------------
       Contract Modal & Form
       ---------------------------------------------------- */
    const contractModal = $('#contractModal');     // modal root
    const contractForm = $('#frmContract');        // form

    const fmContractId = $('#contractId');
    const fmContractRowVersion = $('#contractRowVersion');
    const fmContractStart = $('#contractStartDate');
    const fmContractEnd = $('#contractEndDate');
    const fmContractService = $('#contractServiceName');
    const fmContractTariff = $('#contractTariffType');
    const fmContractStatus = $('#contractStatus');
    const fmContractIsActive = $('#contractIsActive');

    let editingContract = null;

    /* ----------------------------------------------------
       Branch Modal & Form
       ---------------------------------------------------- */
    const branchModal = $('#branchModal');         // modal root
    const branchForm = $('#frmBranch');            // form

    const fmBranchId = $('#branchId');
    const fmBranchRowVersion = $('#branchRowVersion');
    const fmBranchType = $('#branchType');
    const fmBranchCode = $('#branchCode');
    const fmBranchName = $('#branchName');
    const fmBranchCity = $('#branchCity');
    const fmBranchDistrict = $('#branchDistrict');
    const fmBranchPhone = $('#branchPhone');
    const fmBranchEmail = $('#branchEmail');
    const fmBranchIsActive = $('#branchIsActive');

    let editingBranch = null;

    /* ----------------------------------------------------
       Yardımcı: Vergi Dairesi İl / Daire doldurma
       ---------------------------------------------------- */
    function fillTaxOfficeCityOptions() {
        const list = window.vergiDaireIlList || [];
        fmTaxOfficeCity.empty().append('<option value="">Seçiniz</option>');
        list.forEach(x => {
            const val = x.VergiDairesiIl || x.vergiDairesiIl || '';
            fmTaxOfficeCity.append(`<option value="${val}">${val}</option>`);
        });
    }

    function fillTaxOfficeOptions() {
        // Şimdilik il'e göre filtre yok, tüm liste
        const list = window.vergiDaireList || [];
        fmTaxOffice.empty().append('<option value="">Seçiniz</option>');
        list.forEach(v => {
            fmTaxOffice.append(`<option value="${v}">${v}</option>`);
        });
    }

    fmTaxOfficeCity.on('change', function () {
        // Eğer ileride ile göre filtrelenecekse burada yapılabilir
        fillTaxOfficeOptions();
    });

    /* ----------------------------------------------------
       Firma Bilgisi YÜKLE
       ---------------------------------------------------- */
    async function loadCompany() {
        try {
            const data = await CompanyInformationsApi.list();
            if (!data || data.length === 0) {
                currentCompanyId = null;
                fmId.val('');
                return;
            }

            const c = data[0];
            const id = c.id || c.Id || '';
            currentCompanyId = id ? Number(id) : null;
            fmId.val(id || '');

            // VKN / TCKN
            fmVkn.val(
                c.vknTckn ||
                c.taxNo ||
                ''
            );

            // Müşteri Adı
            fmName.val(
                c.musteriAdi ||
                c.companyName ||
                ''
            );

            // Ünvan
            fmTitle.val(
                c.unvan ||
                c.title ||
                ''
            );

            // KEP
            fmKep.val(
                c.kepAdresi ||
                c.kepAddress ||
                ''
            );

            // Ad / Soyad (*Tckn)
            fmPersonName.val(c.ad || c.personName || '');
            fmPersonSurname.val(c.soyad || c.personSurname || '');

            // Kurum Türü
            const kurumTuru =
                c.kurumTuru !== undefined && c.kurumTuru !== null
                    ? c.kurumTuru
                    : (c.institutionType ?? 1);
            fmKurumTuru.val(String(kurumTuru));

            // Kurumsal e-Posta
            fmCorporateMail.val(
                c.kurumsalEposta ||
                c.corporateEmail ||
                ''
            );

            // Vergi Dairesi İl
            fillTaxOfficeCityOptions();
            const taxCity =
                c.vergiDairesiIl ||
                c.taxOfficeCity ||
                '';
            fmTaxOfficeCity.val(taxCity || '').trigger('change');

            // Vergi Dairesi
            fillTaxOfficeOptions();
            const taxOffice =
                c.vergiDairesi ||
                c.taxOffice ||
                '';
            fmTaxOffice.val(taxOffice || '');

            // Müşteri Temsilcisi
            fmCustomerRep.val(
                c.musteriTemsilcisi ||
                c.customerRepresentative ||
                ''
            );

            // Sorumlu
            fmRespTckn.val(
                c.sorumluTckn ||
                c.responsibleTckn ||
                ''
            );
            fmRespAd.val(
                c.sorumluAd ||
                c.responsibleName ||
                ''
            );
            fmRespSoyad.val(
                c.sorumluSoyad ||
                c.responsibleSurname ||
                ''
            );
            fmRespPhone.val(
                c.sorumluCepTel ||
                c.responsiblePhone ||
                ''
            );
            fmRespEmail.val(
                c.sorumluEPosta ||
                c.responsibleEmail ||
                ''
            );

            // Kaydeden
            fmRegName.val(
                c.kaydedenAd ||
                c.registrarName ||
                ''
            );
            fmRegSurname.val(
                c.kaydedenSoyad ||
                c.registrarSurname ||
                ''
            );
            fmRegPhone.val(
                c.kaydedenTel ||
                c.registrarPhone ||
                ''
            );

            // Sözleşme & Şube listelerini yükle
            if (currentCompanyId) {
                await loadContracts(currentCompanyId);
                await loadBranches(currentCompanyId);
            } else {
                gridContracts.html('');
                gridBranches.html('');
            }

        } catch (err) {
            if (window.toastr) toastr.error('Firma bilgileri yüklenemedi.');
            console.error('Firma bilgileri yüklenemedi:', err);
        }
    }

    /* ----------------------------------------------------
       Contract LIST
       ---------------------------------------------------- */
    async function loadContracts(companyId) {
        const cid = companyId || currentCompanyId;
        if (!cid) {
            gridContracts.html('<tr><td colspan="5">Firma seçilmedi.</td></tr>');
            return;
        }

        try {
            const data = await ContractApi.list(cid);
            const rows = (data || []).map(x => {
                const id = x.id || x.Id || '';
                const start = x.startDate || x.StartDate || null;
                const end = x.endDate || x.EndDate || null;

                return `
                    <tr>
                        <td>
                            ${start ? formatDateDisplay(start) : ''} - 
                            ${end ? formatDateDisplay(end) : ''}
                        </td>
                        <td>${toStr(x.serviceName || x.ServiceName || '')}</td>
                        <td>${toStr(x.tariffType || x.TariffType || '')}</td>
                        <td>${toStr(x.status || x.Status || '')}</td>
                        <td>
                            <button class="btn btn-sm btn-warning act-contract-edit" data-id="${id}">
                                <i class="fa fa-edit"></i>
                            </button>
                            <button class="btn btn-sm btn-danger act-contract-del" data-id="${id}">
                                <i class="fa fa-trash"></i>
                            </button>
                        </td>
                    </tr>`;
            }).join('');
            gridContracts.html(rows || '');
        } catch (err) {
            console.error('Sözleşmeler yüklenemedi:', err);
            gridContracts.html('<tr><td colspan="5">Veri alınamadı</td></tr>');
        }
    }

    /* ----------------------------------------------------
       Branch LIST
       ---------------------------------------------------- */
    async function loadBranches(companyId) {
        const cid = companyId || currentCompanyId;
        if (!cid) {
            gridBranches.html('<tr><td colspan="8">Firma seçilmedi.</td></tr>');
            return;
        }

        try {
            const data = await BranchApi.list(cid);
            const rows = (data || []).map(x => {
                const id = x.id || x.Id || '';
                return `
                    <tr>
                        <td>${toStr(x.type || x.Type || '')}</td>
                        <td>${toStr(x.code || x.Code || '')}</td>
                        <td>${toStr(x.name || x.Name || '')}</td>
                        <td>${toStr(x.city || x.City || '')}</td>
                        <td>${toStr(x.district || x.District || '')}</td>
                        <td>${toStr(x.phone || x.Phone || '')}</td>
                        <td>${toStr(x.email || x.Email || '')}</td>
                        <td>
                            <button class="btn btn-sm btn-warning act-branch-edit" data-id="${id}">
                                <i class="fa fa-edit"></i>
                            </button>
                            <button class="btn btn-sm btn-danger act-branch-del" data-id="${id}">
                                <i class="fa fa-trash"></i>
                            </button>
                        </td>
                    </tr>`;
            }).join('');
            gridBranches.html(rows || '');
        } catch (err) {
            console.error('Şubeler yüklenemedi:', err);
            gridBranches.html('<tr><td colspan="8">Veri alınamadı</td></tr>');
        }
    }

    /* ----------------------------------------------------
       Firma FORM SUBMIT (CREATE / UPDATE)
       ---------------------------------------------------- */
    formEl.on('submit', async function (e) {
        e.preventDefault();

        const idVal = fmId.val();
        const id = idVal ? Number(idVal) : undefined;

        const dto = {
            id: id,

            // PascalCase (Entity) alanları
            TaxNo: fmVkn.val(),
            CompanyName: fmName.val(),
            Title: fmTitle.val(),
            KepAddress: fmKep.val(),
            ResponsibleTckn: fmRespTckn.val(),
            ResponsibleName: fmRespAd.val(),
            ResponsibleSurname: fmRespSoyad.val(),
            PersonName: fmPersonName.val(),
            PersonSurname: fmPersonSurname.val(),
            InstitutionType: Number(fmKurumTuru.val() || 1),
            CorporateEmail: fmCorporateMail.val(),
            TaxOfficeCity: fmTaxOfficeCity.val(),
            TaxOffice: fmTaxOffice.val(),
            CustomerRepresentative: fmCustomerRep.val(),
            ResponsiblePhone: fmRespPhone.val(),
            ResponsibleEmail: fmRespEmail.val(),
            RegistrarName: fmRegName.val(),
            RegistrarSurname: fmRegSurname.val(),
            RegistrarPhone: fmRegPhone.val(),

            // Eski JSON isimleri de gönderelim (backend hangisini kullanıyorsa)
            vknTckn: fmVkn.val(),
            musteriAdi: fmName.val(),
            unvan: fmTitle.val(),
            kepAdresi: fmKep.val(),
            ad: fmPersonName.val(),
            soyad: fmPersonSurname.val(),
            kurumTuru: Number(fmKurumTuru.val() || 1),
            kurumsalEposta: fmCorporateMail.val(),
            vergiDairesiIl: fmTaxOfficeCity.val(),
            vergiDairesi: fmTaxOffice.val(),
            musteriTemsilcisi: fmCustomerRep.val(),
            sorumluTckn: fmRespTckn.val(),
            sorumluAd: fmRespAd.val(),
            sorumluSoyad: fmRespSoyad.val(),
            sorumluCepTel: fmRespPhone.val(),
            sorumluEPosta: fmRespEmail.val(),
            kaydedenAd: fmRegName.val(),
            kaydedenSoyad: fmRegSurname.val(),
            kaydedenTel: fmRegPhone.val()
        };

        try {
            if (dto.id) {
                await CompanyInformationsApi.update(dto.id, dto);
            } else {
                const created = await CompanyInformationsApi.create(dto);
                const newId = created && (created.id || created.Id);
                if (newId) {
                    fmId.val(newId);
                    currentCompanyId = Number(newId);
                }
            }

            if (window.toastr) toastr.success('Firma bilgileri güncellendi.');
            await loadCompany();
        } catch (err) {
            if (window.toastr) toastr.error('Kaydetme hatası.');
            console.error('CompanyInformations save error:', err);
        }
    });

    /* ----------------------------------------------------
       CONTRACT CRUD
       ---------------------------------------------------- */

    // Yeni Sözleşme
    $('#btnNewContract').on('click', () => {
        if (!currentCompanyId) {
            if (window.toastr) toastr.error('Önce firma kaydını oluşturun.');
            return;
        }
        editingContract = null;
        if (contractForm[0]) contractForm[0].reset();
        fmContractId.val('');
        fmContractRowVersion.val('');
        fmContractIsActive.prop('checked', true);
        contractModal.modal('show');
    });

    // Sözleşme Düzenle
    $('#SozlesmeGrid').on('click', '.act-contract-edit', async function () {
        const id = $(this).data('id');
        if (!id) return;

        try {
            editingContract = await ContractApi.get(id);

            const start = editingContract.startDate || editingContract.StartDate || null;
            const end = editingContract.endDate || editingContract.EndDate || null;

            fmContractId.val(editingContract.id || editingContract.Id || id);
            fmContractStart.val(start ? formatDateDisplay(start) : '');
            fmContractEnd.val(end ? formatDateDisplay(end) : '');
            fmContractService.val(editingContract.serviceName || editingContract.ServiceName || '');
            fmContractTariff.val(editingContract.tariffType || editingContract.TariffType || '');
            fmContractStatus.val(editingContract.status || editingContract.Status || '');

            const isActive =
                editingContract.isActive !== undefined
                    ? editingContract.isActive
                    : (editingContract.IsActive !== undefined
                        ? editingContract.IsActive
                        : true);
            fmContractIsActive.prop('checked', !!isActive);

            const rvB64 = getRowVersionB64FromEntity(editingContract);
            fmContractRowVersion.val(rvB64 || '');

            contractModal.modal('show');
        } catch (err) {
            if (window.toastr) toastr.error('Sözleşme bilgisi alınamadı.');
            console.error('Contract get error:', err);
        }
    });

    // Sözleşme Sil
    $('#SozlesmeGrid').on('click', '.act-contract-del', async function () {
        const id = $(this).data('id');
        if (!id) return;
        if (!confirm('Bu sözleşme kaydı silinsin mi?')) return;

        try {
            await ContractApi.remove(id);
            if (window.toastr) toastr.success('Sözleşme silindi.');
            await loadContracts(currentCompanyId);
        } catch (err) {
            if (window.toastr) toastr.error('Sözleşme silinemedi.');
            console.error('Contract delete error:', err);
        }
    });

    // Sözleşme Kaydet (create/update)
    contractForm.on('submit', async function (e) {
        e.preventDefault();
        if (!currentCompanyId) {
            if (window.toastr) toastr.error('Önce firma kaydını oluşturun.');
            return;
        }

        const idStr = fmContractId.val();
        const isNew = !idStr;
        const cid = currentCompanyId;

        const startIso = parseDateInput(fmContractStart.val());
        const endIso = parseDateInput(fmContractEnd.val());

        const dto = {
            CompanyInformationId: cid,
            StartDate: startIso,
            EndDate: endIso,
            ServiceName: fmContractService.val(),
            TariffType: fmContractTariff.val(),
            Status: fmContractStatus.val(),
            IsActive: fmContractIsActive.is(':checked')
        };

        if (isNew) {
            Object.assign(dto, buildBaseForCreate());
        } else {
            const base = buildBaseForUpdate(editingContract || {});
            dto.Id = Number(idStr);
            Object.assign(dto, base);
        }

        const rv = (fmContractRowVersion.val() || '').trim();
        if (!isNew && rv) {
            dto.RowVersion = rv;
        }

        try {
            let saved;
            if (isNew) {
                saved = await ContractApi.create(dto);
            } else {
                saved = await ContractApi.update(dto.Id, dto);
            }
            editingContract = saved;

            if (window.toastr) toastr.success('Sözleşme kaydedildi.');
            contractModal.modal('hide');
            await loadContracts(currentCompanyId);
        } catch (err) {
            if (window.toastr) toastr.error('Sözleşme kaydedilemedi.');
            console.error('Contract save error:', err);
        }
    });

    /* ----------------------------------------------------
       BRANCH CRUD
       ---------------------------------------------------- */

    // Yeni Şube
    $('#btnNewBranch').on('click', () => {
        if (!currentCompanyId) {
            if (window.toastr) toastr.error('Önce firma kaydını oluşturun.');
            return;
        }
        editingBranch = null;
        if (branchForm[0]) branchForm[0].reset();
        fmBranchId.val('');
        fmBranchRowVersion.val('');
        fmBranchIsActive.prop('checked', true);
        branchModal.modal('show');
    });

    // Şube Düzenle
    $('#SubeGrid').on('click', '.act-branch-edit', async function () {
        const id = $(this).data('id');
        if (!id) return;

        try {
            editingBranch = await BranchApi.get(id);

            fmBranchId.val(editingBranch.id || editingBranch.Id || id);
            fmBranchType.val(editingBranch.type || editingBranch.Type || '');
            fmBranchCode.val(editingBranch.code || editingBranch.Code || '');
            fmBranchName.val(editingBranch.name || editingBranch.Name || '');
            fmBranchCity.val(editingBranch.city || editingBranch.City || '');
            fmBranchDistrict.val(editingBranch.district || editingBranch.District || '');
            fmBranchPhone.val(editingBranch.phone || editingBranch.Phone || '');
            fmBranchEmail.val(editingBranch.email || editingBranch.Email || '');

            const isActive =
                editingBranch.isActive !== undefined
                    ? editingBranch.isActive
                    : (editingBranch.IsActive !== undefined
                        ? editingBranch.IsActive
                        : true);
            fmBranchIsActive.prop('checked', !!isActive);

            const rvB64 = getRowVersionB64FromEntity(editingBranch);
            fmBranchRowVersion.val(rvB64 || '');

            branchModal.modal('show');
        } catch (err) {
            if (window.toastr) toastr.error('Şube bilgisi alınamadı.');
            console.error('Branch get error:', err);
        }
    });

    // Şube Sil
    $('#SubeGrid').on('click', '.act-branch-del', async function () {
        const id = $(this).data('id');
        if (!id) return;
        if (!confirm('Bu şube kaydı silinsin mi?')) return;

        try {
            await BranchApi.remove(id);
            if (window.toastr) toastr.success('Şube silindi.');
            await loadBranches(currentCompanyId);
        } catch (err) {
            if (window.toastr) toastr.error('Şube silinemedi.');
            console.error('Branch delete error:', err);
        }
    });

    // Şube Kaydet (create/update)
    branchForm.on('submit', async function (e) {
        e.preventDefault();
        if (!currentCompanyId) {
            if (window.toastr) toastr.error('Önce firma kaydını oluşturun.');
            return;
        }

        const idStr = fmBranchId.val();
        const isNew = !idStr;
        const cid = currentCompanyId;

        const dto = {
            CompanyInformationId: cid,
            Type: fmBranchType.val(),
            Code: fmBranchCode.val(),
            Name: fmBranchName.val(),
            City: fmBranchCity.val(),
            District: fmBranchDistrict.val(),
            Phone: fmBranchPhone.val(),
            Email: fmBranchEmail.val(),
            IsActive: fmBranchIsActive.is(':checked')
        };

        if (isNew) {
            Object.assign(dto, buildBaseForCreate());
        } else {
            const base = buildBaseForUpdate(editingBranch || {});
            dto.Id = Number(idStr);
            Object.assign(dto, base);
        }

        const rv = (fmBranchRowVersion.val() || '').trim();
        if (!isNew && rv) {
            dto.RowVersion = rv;
        }

        try {
            let saved;
            if (isNew) {
                saved = await BranchApi.create(dto);
            } else {
                saved = await BranchApi.update(dto.Id, dto);
            }
            editingBranch = saved;

            if (window.toastr) toastr.success('Şube kaydedildi.');
            branchModal.modal('hide');
            await loadBranches(currentCompanyId);
        } catch (err) {
            if (window.toastr) toastr.error('Şube kaydedilemedi.');
            console.error('Branch save error:', err);
        }
    });

    /* ----------------------------------------------------
       INIT
       ---------------------------------------------------- */
    if ($.fn.select2) {
        $('.select2').select2({ width: '100%' });
    }

    fillTaxOfficeCityOptions();
    fillTaxOfficeOptions();

    loadCompany();
});
