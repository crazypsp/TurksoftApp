import { UserApi } from '../Entites/index.js';

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
    // Güvenli origin şart (https). Eski tarayıcılar desteklemeyebilir.
    if (!window.crypto || !window.crypto.subtle) {
        throw new Error('Tarayıcınız WebCrypto (PBKDF2) desteklemiyor.');
    }

    // 1) Rastgele salt
    const salt = new Uint8Array(SALT_LEN);
    window.crypto.getRandomValues(salt);

    // 2) PBKDF2 ile türet
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

    // 3) Base64 ve format
    const saltB64 = toBase64(salt);
    const dkB64 = toBase64(bits);
    return `PBKDF2$${PBKDF2_ITER}$${saltB64}$${dkB64}`;
}

$(document).ready(function () {
    const grid = $('#tblUsers tbody');
    const modal = $('#userModal');

    // Form alanları
    // Modal içindeki form'u hedefle; yoksa ilk form'a düş
    const form = $('#userModal form').length ? $('#userModal form') : $('form').first();
    const fmId = $('#Id');
    const fmName = $('#Username');
    const fmPass = $('#Password');
    const fmEmail = $('#Email');

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
                fmId.val(t.id);
                fmName.val(t.username || '');
                fmEmail.val(t.email || '');
                fmPass.val(''); // Güvenlik: şifre alanını boş aç
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

    $('#btnNew').on('click', () => {
        fmId.val('');
        fmName.val('');
        fmEmail.val('');
        fmPass.val(''); // yeni kullanıcı için şifre girilecek
        modal.modal('show');
    });

    form.on('submit', async e => {
        e.preventDefault();

        // DTO: Şifreyi koşullu ekleyeceğiz
        const dto = {
            id: fmId.val() || undefined,
            Username: fmName.val(),
            Email: fmEmail.val()
            // PasswordHash sonradan eklenecek
        };

        const plain = (fmPass.val() || '').trim();

        // Yeni kullanıcı ise parola zorunlu
        if (!dto.id && !plain) {
            alert('Yeni kullanıcı için parola zorunludur.');
            return;
        }

        // Parola girildiyse PBKDF2 üret ve ekle
        if (plain) {
            try {
                dto.PasswordHash = await makePasswordHash(plain);
            } catch (pbErr) {
                console.error('Parola hash üretilemedi:', pbErr);
                alert(pbErr.message || 'Parola hash üretilirken hata oluştu.');
                return;
            }
        }
        // Not: Parola boşsa PasswordHash göndermiyoruz; backend mevcut hash’i korur.

        try {
            if (dto.id) {
                await UserApi.update(dto.id, dto);
            } else {
                await UserApi.create(dto);
            }
            // Güvenlik için input'u temizle
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
