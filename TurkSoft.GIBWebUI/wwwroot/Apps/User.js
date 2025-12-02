import { UserApi, FirmaApi } from '../Entites/index.js';

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
   RowVersion yardımcıları
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
   Firma DTO helper (UserId bağlamak için)
   ============================================================ */

/**
 * Firma entity'sinden, sadece UserId değişmiş bir update DTO'su üretir.
 * Diğer alanları DB'den gelen haliyle korur.
 */
function buildGibFirmUpdateDtoForAssignUser(firmEntity, assignedUserId) {
    const nowIso = new Date().toISOString();
    const id = firmEntity.id || firmEntity.Id;

    const rvB64 = getRowVersionB64FromEntity(firmEntity);

    const createdAt = firmEntity.createdAt || firmEntity.CreatedAt || nowIso;
    const isActive = (firmEntity.isActive !== undefined
        ? firmEntity.isActive
        : (firmEntity.IsActive !== undefined ? firmEntity.IsActive : true));

    const deleteDate = firmEntity.deleteDate || firmEntity.DeleteDate || null;
    const deletedByUserId = firmEntity.deletedByUserId || firmEntity.DeletedByUserId || null;
    const createdByUserId = firmEntity.createdByUserId || firmEntity.CreatedByUserId || 1;
    const updatedByUserId = 1;

    return {
        // BaseEntity
        UserId: assignedUserId,  // 🔥 Sadece bu alan değişiyor
        IsActive: isActive,
        DeleteDate: deleteDate,
        DeletedByUserId: deletedByUserId,
        CreatedAt: createdAt,
        UpdatedAt: nowIso,
        CreatedByUserId: createdByUserId,
        UpdatedByUserId: updatedByUserId,
        RowVersion: rvB64,

        // Key
        Id: id,

        // Firma alanları - hepsi mevcut değerlerle korunuyor
        Title: firmEntity.title || firmEntity.Title || "",
        TaxNo: firmEntity.taxNo || firmEntity.TaxNo || "",
        TaxOffice: firmEntity.taxOffice || firmEntity.TaxOffice || "",
        CommercialRegistrationNo: firmEntity.commercialRegistrationNo || firmEntity.CommercialRegistrationNo || "",
        MersisNo: firmEntity.mersisNo || firmEntity.MersisNo || "",
        AddressLine: firmEntity.addressLine || firmEntity.AddressLine || "",
        City: firmEntity.city || firmEntity.City || "",
        District: firmEntity.district || firmEntity.District || "",
        Country: firmEntity.country || firmEntity.Country || "",
        PostalCode: firmEntity.postalCode || firmEntity.PostalCode || "",
        Phone: firmEntity.phone || firmEntity.Phone || "",
        Email: firmEntity.email || firmEntity.Email || "",
        GibAlias: firmEntity.gibAlias || firmEntity.GibAlias || "",
        ApiKey: firmEntity.apiKey || firmEntity.ApiKey || "",
        IsEInvoiceRegistered: firmEntity.isEInvoiceRegistered ?? firmEntity.IsEInvoiceRegistered ?? false,
        IsEArchiveRegistered: firmEntity.isEArchiveRegistered ?? firmEntity.IsEArchiveRegistered ?? false
    };
}

/* ============================================================
   UI & API bağlama
   ============================================================ */

$(document).ready(function () {
    const grid = $('#tblUsers tbody');
    const modal = $('#userModal');

    const form = $('#userModal form').length ? $('#userModal form') : $('form').first();
    const fmId = $('#Id');
    const fmName = $('#Username');
    const fmPass = $('#Password');
    const fmEmail = $('#Email');
    const fmIsActive = $('#IsActive');
    const fmRowVersion = $('#RowVersionBase64');

    const ddlFirm = $('#GibFirmId'); // seçilen firma Id'sini okumak için

    let gibFirmCache = null;

    async function loadGibFirms() {
        if (gibFirmCache) return gibFirmCache;
        try {
            const list = await FirmaApi.list();
            gibFirmCache = list || [];
            return gibFirmCache;
        } catch (err) {
            console.error('Firma listesi alınamadı:', err);
            gibFirmCache = [];
            return gibFirmCache;
        }
    }

    /**
     * Firma dropdown'unu doldurur.
     * userId + currentFirmId:
     *   - currentFirmId varsa onu seç,
     *   - yoksa firmalardan UserId == userId olan varsa onu seç,
     *   - hiçbiri yoksa "Seçiniz" seçili kalsın.
     */
    async function fillFirmDropdownForUser(userId, currentFirmId) {
        const firms = await loadGibFirms();

        const select = document.getElementById('GibFirmId');
        if (!select) {
            console.warn('#GibFirmId select elementi bulunamadı.');
            return;
        }

        // Tüm option'ları temizle
        while (select.options.length > 0) {
            select.remove(0);
        }

        // "Seçiniz" option'u
        const defaultOpt = new Option('Seçiniz', '');
        select.add(defaultOpt);

        // Firmaları ekle
        if (Array.isArray(firms)) {
            firms.forEach(f => {
                const id = f.id || f.Id;
                const title = f.title || f.Title || '';
                const text = `${id} - ${title}`;
                const opt = new Option(text, id);
                select.add(opt);
            });
        }

        // Varsayılan seçim: önce user'ın kendi GibFirmId'si
        let selectedValue = currentFirmId ? String(currentFirmId) : '';

        // Yoksa, firmalar içinde UserId eşleşeni ara
        if (!selectedValue && userId) {
            const match = (Array.isArray(firms) ? firms : []).find(f =>
                f.userId === userId ||
                f.UserId === userId
            );
            if (match) {
                selectedValue = String(match.id || match.Id);
            }
        }

        // Son halde seçim yoksa Seçiniz
        select.value = selectedValue || '';
    }

    async function loadTable() {
        try {
            const data = await UserApi.list();
            grid.html((data || []).map(t => `
                <tr data-id="${t.id}">
                    <td>${t.id}</td>
                    <td>${t.username || ''}</td>
                    <td>${t.email || ''}</td>
                    <td class="text-center" style="width:120px;">
                        <button class="btn btn-warning btn-sm act-edit"><i class="fa fa-edit"></i></button>
                        <button class="btn btn-danger btn-sm act-del"><i class="fa fa-trash"></i></button>
                    </td>
                </tr>
            `).join(''));
            bindActions();
        } catch (err) {
            console.error('Kullanıcı listesi yüklenemedi:', err);
            alert(err.message || 'Liste yüklenemedi.');
        }
    }

    function bindActions() {
        $('.act-edit').off('click').on('click', async function () {
            const id = $(this).closest('tr').data('id');
            try {
                const t = await UserApi.get(id);

                const userId = t.id || t.Id;
                const userFirmId = t.gibFirmId || t.GibFirmId || null;

                fmId.val(userId);
                fmName.val(t.username || t.Username || '');
                fmEmail.val(t.email || t.Email || '');
                fmPass.val('');

                const isActive = (t.isActive !== undefined
                    ? t.isActive
                    : (t.IsActive !== undefined ? t.IsActive : true));
                fmIsActive.prop('checked', !!isActive);

                const rvB64 = getRowVersionB64FromEntity(t);
                fmRowVersion.val(rvB64 || '');

                await fillFirmDropdownForUser(userId, userFirmId);

                $('#userModalLabel').text('Kullanıcı Güncelle');
                modal.modal('show');
            } catch (err) {
                console.error('Kayıt getirilemedi:', err);
                alert(err.message || 'Kayıt getirilemedi.');
            }
        });

        $('.act-del').off('click').on('click', async function () {
            const id = $(this).closest('tr').data('id');
            if (!confirm('Bu kullanıcı kaydı silinsin mi?')) return;
            try {
                await UserApi.remove(id);
                await loadTable();
            } catch (err) {
                console.error('Silme hatası:', err);
                alert(err.message || 'Kayıt silinemedi.');
            }
        });
    }

    $('#btnNew').on('click', async () => {
        fmId.val('');
        fmName.val('');
        fmEmail.val('');
        fmPass.val('');
        fmIsActive.prop('checked', true);
        fmRowVersion.val('');

        // Yeni kayıt: hiçbir firma seçili olmasın → Seçiniz
        await fillFirmDropdownForUser(null, null);

        $('#userModalLabel').text('Yeni Kullanıcı');
        modal.modal('show');
    });

    /**
     * Kullanıcı kaydedildikten sonra, seçilen firmayı bu kullanıcıya bağlar:
     *   Firma.UserId = userId
     * (User tarafındaki GibFirmId zaten DTO içinde gönderiliyor)
     */
    async function assignUserToSelectedFirm(userId) {
        const firmIdStr = ddlFirm.val();
        if (!firmIdStr) {
            return; // firma seçilmemişse firmaya dokunmuyoruz
        }

        const firmId = Number(firmIdStr);
        if (!Number.isFinite(firmId) || firmId <= 0) {
            console.warn('Geçersiz firma Id:', firmIdStr);
            return;
        }

        try {
            // 1) Firmayı DB'den çek
            const firmEntity = await FirmaApi.get(firmId);

            // 2) UserId alanını bu kullanıcı Id'si ile güncelleyecek DTO'yu hazırla
            const updateDto = buildGibFirmUpdateDtoForAssignUser(firmEntity, userId);
            try {
                // 3) Update çağrısı
                await FirmaApi.update(updateDto.Id, updateDto);
            } catch (e) {
                console.log(e.message);
            }
            
        } catch (err) {
            console.error('Seçilen firmaya kullanıcı bağlanırken hata:', err);
            alert('Kullanıcı kaydedildi, ancak firmaya kullanıcı bağlanırken hata oluştu.');
        }
    }

    form.on('submit', async e => {
        e.preventDefault();

        const username = (fmName.val() || '').trim();
        const email = (fmEmail.val() || '').trim();
        const plain = (fmPass.val() || '').trim();

        if (!username) {
            alert('Kullanıcı adı zorunludur.');
            return;
        }
        if (!email) {
            alert('E-posta zorunludur.');
            return;
        }

        // 🔹 Dropdown’dan seçilen firma Id'si (User.GibFirmId için)
        const firmIdStr = ddlFirm.val();
        const firmId = firmIdStr ? Number(firmIdStr) : null;

        const dto = {
            id: fmId.val() ? Number(fmId.val()) : 0,
            Username: username,
            Email: email,
            IsActive: fmIsActive.is(':checked'),
            GibFirmId: firmId   // 🔥 User tablosundaki GibFirmId alanı
        };

        const rvB64 = (fmRowVersion.val() || '').trim();
        if (dto.id && rvB64) {
            dto.RowVersion = rvB64;
        }

        // Yeni kullanıcı için parola zorunlu
        if (!dto.id && !plain) {
            alert('Yeni kullanıcı için parola zorunludur.');
            return;
        }

        if (plain) {
            try {
                dto.PasswordHash = await makePasswordHash(plain);
            } catch (pbErr) {
                console.error('Parola hash üretilemedi:', pbErr);
                alert(pbErr.message || 'Parola hash üretilirken hata oluştu.');
                return;
            }
        }

        try {
            let savedUser;
            if (dto.id) {
                savedUser = await UserApi.update(dto.id, dto);
            } else {
                savedUser = await UserApi.create(dto);
            }

            // Kullanıcının gerçek Id'si
            const userId =
                (savedUser && (savedUser.id || savedUser.Id)) ||
                dto.id ||
                null;

            // Firma seçiliyse, o firmaya bu kullanıcıyı bağla (Firma.UserId = userId)
            if (userId) {
                await assignUserToSelectedFirm(userId);
            }

            fmPass.val('');
            modal.modal('hide');
            await loadTable();
        } catch (err) {
            console.error('Kayıt hatası:', err);
            alert(err.message || 'Kayıt yapılamadı.');
        }
    });

    loadTable();
});
