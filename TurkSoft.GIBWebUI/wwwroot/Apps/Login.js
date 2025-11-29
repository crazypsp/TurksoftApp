// /apps/login-glue.js
import { signIn, getSession, clearAllSessions } from '../Service/Login.js';
import { FirmaApi } from '../Entites/index.js';

function ensureErrorBox() {
    let el = document.getElementById('loginError');
    if (!el) {
        el = document.createElement('div');
        el.id = 'loginError';
        el.className = 'alert alert-danger';
        el.style.display = 'none';
        const form = document.getElementById('loginForm');
        if (form) form.prepend(el);
    }
    return el;
}

function isValidEmail(email) {
    return /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/.test(email);
}

function setInvalid(input, message) {
    if (!input) return;
    input.classList.add('is-invalid');

    let fb = input.nextElementSibling && input.nextElementSibling.classList?.contains('invalid-feedback')
        ? input.nextElementSibling
        : null;

    if (!fb) {
        fb = document.createElement('div');
        fb.className = 'invalid-feedback';
        input.insertAdjacentElement('afterend', fb);
    }
    fb.textContent = message || '';
}

function clearInvalid(input) {
    if (!input) return;
    input.classList.remove('is-invalid');
}

function wireValidation(emailInput, passInput) {
    const clear = (e) => e.target.classList.remove('is-invalid');
    emailInput?.addEventListener('input', clear);
    passInput?.addEventListener('input', clear);
}

function showError(msg) {
    const box = ensureErrorBox();
    if (msg) {
        box.textContent = msg;
        box.style.display = 'block';
    } else {
        box.textContent = '';
        box.style.display = 'none';
    }
}

// ---------------- LOGIN FLOW ----------------

document.addEventListener('DOMContentLoaded', async () => {
    clearAllSessions();
    const form = document.getElementById('loginForm');
    const emailInput = document.getElementById('Email');
    const passwordInput = document.getElementById('Password');
    const submitBtn = form?.querySelector('button[type="submit"]');
    const homeUrl = form?.getAttribute('data-home-url') || '/';

    wireValidation(emailInput, passwordInput);
    showError('');

    // Otomatik yönlendirme: sadece email & password girildiyse
    try {
        const existing = await getSession({ hydrate: true });
        const email = emailInput?.value?.trim();
        const pass = passwordInput?.value?.trim();

        if (
            existing?.userId &&
            existing?.loginAt &&
            email && pass &&
            isValidEmail(email)
        ) {
            window.location.href = homeUrl;
            return;
        }
    } catch {
        // Sessiz geç
    }

    // Son kullanılan e-mail'i hatırlat
    try {
        const lastMail = sessionStorage.getItem('lastLoginEmail');
        if (lastMail && emailInput && !emailInput.value) {
            emailInput.value = lastMail;
        }
    } catch { }

    // Form submit işlemi
    form?.addEventListener('submit', async (e) => {
        e.preventDefault();
        showError('');

        const email = (emailInput?.value || '').trim();
        const pass = (passwordInput?.value || '').trim();

        let ok = true;

        if (!email) {
            setInvalid(emailInput, 'E-posta zorunludur.');
            ok = false;
        } else if (!isValidEmail(email)) {
            setInvalid(emailInput, 'Geçerli bir e-posta adresi girin.');
            ok = false;
        } else {
            clearInvalid(emailInput);
        }

        if (!pass) {
            setInvalid(passwordInput, 'Şifre zorunludur.');
            ok = false;
        } else {
            clearInvalid(passwordInput);
        }

        if (!ok) return;

        submitBtn?.setAttribute('disabled', 'disabled');
        submitBtn?.classList.add('disabled');

        try {
            // 1) Login
            const res = await signIn(email, pass);
            console.log('Giriş sonucu:', res);

            if (!res?.success) {
                showError(res?.message || 'Giriş başarısız.');
                return;
            }

            // 2) Firma listesi
            const byFirma = await FirmaApi.list();
            console.log('Firma liste sonucu:', byFirma);
            storeFirmaToSession(res, byFirma);

            // 3) Mükellef listesini GİB recipient-zip üzerinden çek ve session'a yaz
            let storedMukellef = [];
            try {
                storedMukellef = await storeMukellefToSession();
                console.log('[GIB] Login sonrası mükellef listesi yüklendi. Kaydedilen adet:', storedMukellef.length);
            } catch (err) {
                console.error('[GIB] Login sonrası mükellef yüklenirken hata:', err);
            }

            // 4) Son kullanılan e-posta'yı sakla
            try { sessionStorage.setItem('lastLoginEmail', email); } catch { }

            // 5) Artık güvenle home'a yönlenebiliriz
            window.location.href = homeUrl;
        } catch (err) {
            console.error('Login error:', err);
            showError(err?.message || 'Beklenmeyen bir hata oluştu.');
        } finally {
            submitBtn?.removeAttribute('disabled');
            submitBtn?.classList.remove('disabled');
        }
    });
});

// ---------------- YARDIMCI FONKSİYONLAR ----------------

function storeFirmaToSession(result, firmaList) {
    const user = result?.user;
    if (!user) {
        console.warn('storeFirmaToSession: user bulunamadı.');
        return;
    }

    const firma = Array.isArray(firmaList)
        ? firmaList.find(f => f.userId === user.id)
        : null;

    if (firma) {
        sessionStorage.setItem('Firma', JSON.stringify(firma));
        console.log('Firma sessiona yazıldı:', firma);
    } else {
        console.warn('storeFirmaToSession: user.id için firma bulunamadı:', user.id);
    }
}

/**
 * Büyük array'leri sessionStorage'a yazarken quota hatasını
 * yönetmek için yardımcı fonksiyon.
 * QuotaExceededError alırsa listeyi küçülterek tekrar dener.
 */
function saveToSessionWithQuota(key, value) {
    if (!window.sessionStorage) return value;

    // Sadece array için küçültme yapıyoruz
    if (!Array.isArray(value)) {
        try {
            sessionStorage.setItem(key, JSON.stringify(value));
        } catch (e) {
            console.error('[Storage] setItem hata (array olmayan):', e);
        }
        return value;
    }

    let current = value;
    while (true) {
        try {
            const json = JSON.stringify(current);
            sessionStorage.setItem(key, json);
            if (current.length !== value.length) {
                console.warn('[Storage] Quota nedeniyle liste kısaltıldı. Kaydedilen adet:', current.length);
            }
            return current;
        } catch (e) {
            const isQuota =
                e &&
                (e.name === 'QuotaExceededError' ||
                    e.code === 22 ||    // Chrome
                    e.code === 1014);   // Firefox

            if (!isQuota) {
                console.error('[Storage] setItem beklenmeyen hata:', e);
                return value;
            }

            if (current.length <= 1) {
                console.error('[Storage] QuotaExceededError: 1 kayıt bile kaydedilemedi, vazgeçiliyor.');
                try { sessionStorage.removeItem(key); } catch { }
                return [];
            }

            const newLen = Math.max(1, Math.floor(current.length * 0.7));
            console.warn('[Storage] QuotaExceededError, liste küçültülüyor. Eski:', current.length, 'Yeni:', newLen);
            current = current.slice(0, newLen);
        }
    }
}

/**
 * GİB'ten gelen ham listeyi daha küçük bir yapıya çevirir.
 * Sadece Identifier, Title ve Alias alanlarını saklıyoruz.
 */
function buildSlimMukellefList(list) {
    if (!Array.isArray(list)) return [];
    return list.map((raw, idx) => {
        if (!raw || typeof raw !== 'object') raw = {};

        const identifier =
            raw.Identifier ||
            raw.identifier ||
            raw.Vkn ||
            raw.vkn ||
            '';

        const title =
            raw.Title ||
            raw.title ||
            '';

        const alias =
            raw.Alias ||
            raw.alias ||
            raw.GibAlias ||
            raw.gibAlias ||
            '';

        const id = raw.Id || raw.id || identifier || (idx + 1);

        return {
            id,
            Identifier: (identifier || '').toString().trim(),
            Title: (title || '').toString().trim(),
            Alias: (alias || '').toString().trim()
        };
    });
}

// Mükellef (GİB kullanıcıları / alıcı listesi) ZIP'ini indir,
// içindeki gibusers_invoice_receipt_list.json'u oku,
// SLIM listeyi sessionStorage'a "MukellefList" olarak yaz.
async function storeMukellefToSession() {
    let list = [];

    try {
        const baseUrl = window.gibPortalApiBaseUrl;
        if (!baseUrl) {
            throw new Error('gibPortalApiBaseUrl tanımlı değil. Mukellef listesi alınamadı.');
        }

        const normBase = baseUrl.endsWith('/') ? baseUrl : baseUrl + '/';
        const url = normBase + 'TurkcellEFatura/gibuser/recipient-zip';

        console.log('[GIB] Mükellef ZIP isteği başlıyor:', url);

        const response = await fetch(url, {
            method: 'GET',
            headers: {
                'Accept': '*/*'
            }
        });

        console.log('[GIB] Mükellef ZIP isteği döndü. Status:', response.status, response.statusText);

        if (!response.ok) {
            let errorText = '';
            try {
                errorText = await response.text();
            } catch { /* ignore */ }

            throw new Error(
                'GİB API isteği başarısız. Status: '
                + response.status + ' ' + response.statusText
                + (errorText ? ' - ' + errorText : '')
            );
        }

        const arrayBuffer = await response.arrayBuffer();
        console.log('[GIB] ZIP boyutu (byte):', arrayBuffer.byteLength);

        const zip = await JSZip.loadAsync(arrayBuffer);

        const targetJsonName = 'gibusers_invoice_receipt_list.json';

        let jsonFile = zip.file(targetJsonName);
        if (!jsonFile) {
            const candidates = zip.file(/gibusers_invoice_receipt_list\.json$/i);
            if (candidates && candidates.length) {
                jsonFile = candidates[0];
            }
        }
        if (!jsonFile) {
            const anyJson = zip.file(/\.json$/i);
            if (anyJson && anyJson.length) {
                console.warn('[GIB] Hedef isim bulunamadı, ilk JSON dosyası kullanılacak:', anyJson[0].name);
                jsonFile = anyJson[0];
            }
        }

        if (!jsonFile) {
            throw new Error('ZIP içerisinde JSON dosyası bulunamadı.');
        }

        console.log('[GIB] Kullanılacak JSON dosyası:', jsonFile.name);

        let jsonText = await jsonFile.async('string');

        // BOM & baştaki boşluk temizliği
        if (jsonText.charCodeAt(0) === 0xFEFF) {
            jsonText = jsonText.slice(1);
        }
        jsonText = jsonText.trimStart();

        let data;
        try {
            data = JSON.parse(jsonText);
        } catch (err) {
            console.error('[GIB] Mükellef JSON parse hatası:', err);
            console.debug('[GIB] Gelen JSON text (ilk 200 char):', jsonText.substring(0, 200));
            throw err;
        }

        if (Array.isArray(data)) {
            list = data;
        } else if (Array.isArray(data.items)) {
            list = data.items;
        } else if (Array.isArray(data.results)) {
            list = data.results;
        } else if (Array.isArray(data.list)) {
            list = data.list;
        } else {
            list = [data];
        }
    } catch (err) {
        console.error('[GIB] storeMukellefToSession hata:', err);
        list = [];
    }

    // ❗ Büyük ham listeyi küçült: sadece Identifier / Title / Alias
    const slimList = buildSlimMukellefList(list);
    console.log('[GIB] Ham liste:', list.length, 'Slim liste:', slimList.length);

    // ❗ Quota'yı yöneterek session'a kaydet
    const storedList = saveToSessionWithQuota('MukellefList', slimList);

    console.log('[GIB] MukellefList session\'a yazıldı (login). Kaydedilen adet:', storedList.length);

    return storedList;
}
