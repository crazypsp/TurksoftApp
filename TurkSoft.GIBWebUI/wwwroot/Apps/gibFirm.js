import { FirmaApi, GibUserCreditAccountApi } from '../Entites/index.js';

/**
 * RowVersion hex → base64
 * Ör: "0x00000000000007DA" -> base64
 */
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

// Yeni kayıt için kullanılacak default RowVersion
const DEFAULT_ROWVERSION_HEX = "0x00000000000007E1";
const DEFAULT_ROWVERSION_BASE64 = rowVersionHexToBase64(DEFAULT_ROWVERSION_HEX);

/**
 * API'den gelen entity içinden RowVersion'ı okuyup base64’e çevirir.
 * Hiç yoksa DEFAULT_ROWVERSION_BASE64 döner.
 */
function getRowVersionFromEntityOrDefault(entity) {
    if (!entity) return DEFAULT_ROWVERSION_BASE64;

    let rv =
        entity.rowVersionBase64 || entity.RowVersionBase64 ||
        entity.rowVersion || entity.RowVersion ||
        entity.rowVersionHex || entity.RowVersionHex;

    if (rv && /^0x/i.test(rv)) {
        rv = rowVersionHexToBase64(rv);
    }

    if (!rv) {
        rv = DEFAULT_ROWVERSION_BASE64;
    }
    return rv;
}

/**
 * BaseEntity seti – UserId ve CreatedByUserId sabit 1
 *
 * isNew: true → insert, false → update
 * existing: API'den gelen mevcut GibFirm (update’te)
 * overrideRowVersion: formdan gelen base64 RowVersion (varsa)
 */
function buildBaseEntityForFirm(isNew, nowIso, existing, overrideRowVersion) {
    const ex = existing || {};

    const BASE_USER_ID = 1; // 🔥 İstediğin gibi sabit 1

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

    // Update için mevcut değerlerden doldurup boş kalanları 1 ile tamamla
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

$(document).ready(function () {
    const grid = $('#tblGibFirms tbody');
    const modal = $('#gibFirmModal');
    const form = $('#gibFirmForm');

    const fmId = $('#FirmId');

    // RowVersion hidden: RowVersionBase64 veya RowVersion
    const fmRowVersion =
        $('#RowVersionBase64').length ? $('#RowVersionBase64') :
            ($('#RowVersion').length ? $('#RowVersion') : null);

    const fmTitle = $('#Title');
    const fmTaxNo = $('#TaxNo');
    const fmTaxOffice = $('#TaxOffice');
    const fmCommRegNo = $('#CommercialRegistrationNo');
    const fmMersisNo = $('#MersisNo');
    const fmAddress = $('#AddressLine');
    const fmCity = $('#City');
    const fmDistrict = $('#District');
    const fmCountry = $('#Country');
    const fmPostalCode = $('#PostalCode');
    const fmPhone = $('#Phone');
    const fmEmail = $('#FirmEmail');
    const fmGibAlias = $('#GibAlias');
    const fmApiKey = $('#ApiKey');
    const fmIsEInv = $('#IsEInvoiceRegistered');
    const fmIsEArch = $('#IsEArchiveRegistered');
    const fmInitialCredits = $('#InitialCredits');

    // Edit modunda API'den dönen entity
    let currentFirmEntity = null;

    async function loadTable() {
        try {
            const data = await FirmaApi.list();
            grid.html((data || []).map(f => `
                <tr data-id="${f.id}">
                    <td>${f.title || ''}</td>
                    <td>${f.taxNo || ''}</td>
                    <td>${f.gibAlias || ''}</td>
                    <td>${f.isEInvoiceRegistered ? 'Evet' : 'Hayır'}</td>
                    <td>${f.isEArchiveRegistered ? 'Evet' : 'Hayır'}</td>
                    <td class="text-center">
                        <button class="btn btn-warning btn-sm act-edit-firm">
                            <i class="fa fa-edit"></i>
                        </button>
                        <button class="btn btn-danger btn-sm act-del-firm">
                            <i class="fa fa-trash"></i>
                        </button>
                    </td>
                </tr>
            `).join(''));
            bindActions();
        } catch (err) {
            console.error('Firma listesi yüklenemedi:', err);
            alert(err.message || 'Firma listesi yüklenemedi.');
        }
    }

    function clearForm() {
        currentFirmEntity = null;

        fmId.val('');
        if (fmRowVersion) {
            fmRowVersion.val(DEFAULT_ROWVERSION_BASE64);
        }

        fmTitle.val('');
        fmTaxNo.val('');
        fmTaxOffice.val('');
        fmCommRegNo.val('');
        fmMersisNo.val('');
        fmAddress.val('');
        fmCity.val('');
        fmDistrict.val('');
        fmCountry.val('');
        fmPostalCode.val('');
        fmPhone.val('');
        fmEmail.val('');
        fmGibAlias.val('');
        fmApiKey.val('');
        fmIsEInv.prop('checked', false);
        fmIsEArch.prop('checked', false);
        fmInitialCredits.val('');
    }

    function bindActions() {
        $('.act-edit-firm').off('click').on('click', async function () {
            const id = $(this).closest('tr').data('id');
            try {
                const f = await FirmaApi.get(id);
                currentFirmEntity = f;

                fmId.val(f.id);

                if (fmRowVersion) {
                    const rvB64 = getRowVersionFromEntityOrDefault(f);
                    fmRowVersion.val(rvB64);
                }

                fmTitle.val(f.title || '');
                fmTaxNo.val(f.taxNo || '');
                fmTaxOffice.val(f.taxOffice || '');
                fmCommRegNo.val(f.commercialRegistrationNo || '');
                fmMersisNo.val(f.mersisNo || '');
                fmAddress.val(f.addressLine || '');
                fmCity.val(f.city || '');
                fmDistrict.val(f.district || '');
                fmCountry.val(f.country || '');
                fmPostalCode.val(f.postalCode || '');
                fmPhone.val(f.phone || '');
                fmEmail.val(f.email || '');
                fmGibAlias.val(f.gibAlias || '');
                fmApiKey.val(f.apiKey || '');
                fmIsEInv.prop('checked', !!f.isEInvoiceRegistered);
                fmIsEArch.prop('checked', !!f.isEArchiveRegistered);

                fmInitialCredits.val('');

                $('#gibFirmModalLabel').text('Firma Güncelle');
                modal.modal('show');
            } catch (err) {
                console.error('Firma getirilemedi:', err);
                alert(err.message || 'Firma getirilemedi.');
            }
        });

        $('.act-del-firm').off('click').on('click', async function () {
            const id = $(this).closest('tr').data('id');
            if (!confirm('Bu firma silinsin mi?')) return;
            try {
                await FirmaApi.remove(id);
                await loadTable();
            } catch (err) {
                console.error('Firma silinemedi:', err);
                alert(err.message || 'Firma silinemedi.');
            }
        });
    }

    $('#btnNewFirm').on('click', () => {
        clearForm();
        $('#gibFirmModalLabel').text('Yeni Firma');
        modal.modal('show');
    });

    form.on('submit', async e => {
        e.preventDefault();

        const isNew = !fmId.val();
        const nowIso = new Date().toISOString();

        if (!fmTitle.val()) {
            alert('Ünvan zorunludur.');
            return;
        }

        const creditsRaw = (fmInitialCredits.val() || '').trim();
        const credits = creditsRaw ? parseInt(creditsRaw, 10) : 0;
        if (creditsRaw && (isNaN(credits) || credits < 0)) {
            alert('Başlangıç kontör değeri geçersiz.');
            return;
        }

        const overrideRv = (fmRowVersion && fmRowVersion.val())
            ? fmRowVersion.val().trim()
            : null;

        // BaseEntity seti – UserId/CreatedByUserId/UpdatedByUserId = 1
        const baseEntity = buildBaseEntityForFirm(
            isNew,
            nowIso,
            isNew ? null : currentFirmEntity,
            overrideRv
        );

        // DTO → GibFirm entity ile birebir
        const dto = {
            ...baseEntity,

            Id: isNew ? 0 : Number(fmId.val()),

            Title: fmTitle.val(),
            TaxNo: fmTaxNo.val(),
            TaxOffice: fmTaxOffice.val(),
            CommercialRegistrationNo: fmCommRegNo.val(),
            MersisNo: fmMersisNo.val(),
            AddressLine: fmAddress.val(),
            City: fmCity.val(),
            District: fmDistrict.val(),
            Country: fmCountry.val(),
            PostalCode: fmPostalCode.val(),
            Phone: fmPhone.val(),
            Email: fmEmail.val(),
            GibAlias: fmGibAlias.val(),
            ApiKey: fmApiKey.val(),
            IsEInvoiceRegistered: fmIsEInv.is(':checked'),
            IsEArchiveRegistered: fmIsEArch.is(':checked')
        };

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

            // GibUserCreditAccount da BaseEntity’den türemişse, burada da aynı base’i kullan
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

            modal.modal('hide');
            await loadTable();
        } catch (err) {
            console.error('Firma kaydedilemedi:', err);

            if (err.responseJSON) {
                console.log('API error response:', err.responseJSON);
            } else if (err.responseText) {
                console.log('API error text:', err.responseText);
            }

            alert(err.message || 'Firma kaydedilemedi (API Error 500).');
        }
    });

    loadTable();
});
