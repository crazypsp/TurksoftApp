// /apps/login-glue.js
import { signIn, getSession, clearAllSessions } from '../Service/Login.js';
import { FirmaApi, UserRolesApi, GibUserCreditAccountApi } from '../Entites/index.js';

// ------------------------------------------------------
//  Küçük yardımcılar
// ------------------------------------------------------
function getUserIdFlexible(user) {
    if (!user) return null;
    return (
        user.Id ??
        user.id ??
        user.UserId ??
        user.userId ??
        null
    );
}

function getUserRoleUserId(x) {
    if (!x) return null;
    return (
        x.UserId ??
        x.userId ??
        x.KullaniciId ??
        x.kullaniciId ??
        null
    );
}

function getCreditUserId(x) {
    if (!x) return null;
    return (
        x.UserId ??
        x.userId ??
        x.KullaniciId ??
        x.kullaniciId ??
        null
    );
}

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
    return /^[^\s@]+@[^\s@]{2,}\.[^\s@]{2,}$/.test(email);
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

// ---------------- LOADING BUTON & OVERLAY ----------------

function setButtonLoading(btn, isLoading) {
    if (!btn) return;

    if (isLoading) {
        if (!btn.dataset.originalHtml) {
            btn.dataset.originalHtml = btn.innerHTML;
        }
        btn.disabled = true;
        btn.classList.add('disabled');
        btn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Giriş yapılıyor...';
    } else {
        if (btn.dataset.originalHtml) {
            btn.innerHTML = btn.dataset.originalHtml;
        }
        btn.disabled = false;
        btn.classList.remove('disabled');
    }
}

function setGlobalOverlayLoading(isLoading) {
    // Login sayfasında özel overlay varsa onu kullan, yoksa layout overlay
    var overlay =
        document.getElementById('login-loading-overlay') ||
        document.getElementById('overlayManuel') ||
        document.getElementById('overlay');

    if (!overlay) return;
    overlay.style.display = isLoading ? 'block' : 'none';
}

function setLoadingState(btn, isLoading) {
    setButtonLoading(btn, isLoading);
    setGlobalOverlayLoading(isLoading);
}

// ------------------------------------------------------
//  Kullanıcının Rol Id'sini UserRolesApi'den çek
// ------------------------------------------------------
async function fetchUserRoleIdFromApi(userId) {
    if (!userId) {
        console.warn('[Login] fetchUserRoleIdFromApi: userId yok, 4(Firma) varsayılan.');
        return 4;
    }

    try {
        const res = await UserRolesApi.list();
        const arr = Array.isArray(res) ? res : (res ? [res] : []);

        const forUser = arr.filter(r => {
            const rid = getUserRoleUserId(r);
            const active = (r.IsActive ?? r.isActive);
            const isActive = (active === undefined) ? true : !!active;
            return rid === userId && isActive;
        });

        if (!forUser.length) {
            console.log('[Login] UserRolesApi: user için aktif rol kaydı yok, 4(Firma) varsayılan.');
            return 4;
        }

        const first = forUser[0];
        const rawRoleId = first.RoleId ?? first.roleId ?? first.Id ?? first.id;
        const numRoleId = Number(rawRoleId);

        if (!Number.isFinite(numRoleId) || numRoleId <= 0) {
            console.warn('[Login] UserRolesApi: RoleId sayısal değil, 4(Firma) kullanılıyor. rawRoleId:', rawRoleId);
            return 4;
        }

        console.log('[Login] UserRolesApi: user için RoleId bulundu:', numRoleId);
        return numRoleId;
    } catch (err) {
        console.error('[Login] UserRolesApi.list hata:', err);
        return 4;
    }
}

// ------------------------------------------------------
//  Kullanıcının kredi hesabını GibUserCreditAccountApi'den çek
// ------------------------------------------------------
async function fetchUserCreditsFromApi(userId) {
    if (!userId) {
        console.warn('[Login] fetchUserCreditsFromApi: userId yok.');
        return null;
    }

    try {
        const res = await GibUserCreditAccountApi.list();
        const arr = Array.isArray(res) ? res : (res ? [res] : []);

        const row = arr.find(c => getCreditUserId(c) === userId);
        if (!row) {
            console.log('[Login] GibUserCreditAccountApi: user için kredi kaydı yok.');
            return null;
        }

        try {
            sessionStorage.setItem('UserCreditAccount', JSON.stringify(row));
        } catch { }

        console.log('[Login] Kullanıcının kredi kaydı bulundu ve session\'a yazıldı:', row);
        return row;
    } catch (err) {
        console.error('[Login] GibUserCreditAccountApi.list hata:', err);
        return null;
    }
}

// ------------------------------------------------------
//  Mükellef yenileme isteğini Controller'a at
//  (ağır işi artık sunucu yapacak, JSZip yok)
// ------------------------------------------------------
async function triggerMukellefRefreshOnServer(userId) {
    if (!userId) {
        console.warn('[Login] triggerMukellefRefreshOnServer: userId yok.');
        return;
    }

    try {
        const url = '/Login/RefreshMukellef?userId=' + encodeURIComponent(userId);
        console.log('[Login] Mükellef yenileme isteği gönderiliyor:', url);

        // Controller büyük ZIP işini Task.Run ile arkada yapıyor,
        // biz sadece isteğin başarılı dönmesini bekliyoruz.
        const resp = await fetch(url, {
            method: 'GET',
            headers: { 'Accept': 'application/json' }
        });

        console.log('[Login] RefreshMukellef yanıtı:', resp.status, resp.statusText);
    } catch (err) {
        console.error('[Login] triggerMukellefRefreshOnServer hata:', err);
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

    // Son kullanılan e-mail
    try {
        const lastMail = sessionStorage.getItem('lastLoginEmail');
        if (lastMail && emailInput && !emailInput.value) {
            emailInput.value = lastMail;
        }
    } catch { }

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

        setLoadingState(submitBtn, true);

        try {
            // 1) Login
            const res = await signIn(email, pass);
            console.log('Giriş sonucu:', res);

            if (!res?.success) {
                showError(res?.message || 'Giriş başarısız.');
                return;
            }

            const signedUser = res?.user || null;
            const currentUserId = getUserIdFlexible(signedUser);

            console.log('Login sonrası userId:', currentUserId, 'user:', signedUser);

            if (typeof window !== 'undefined' && currentUserId) {
                window.currentUserId = currentUserId;
            }

            // 2) Rol ID (UserRolesApi) -> sessionStorage
            const apiRoleId = await fetchUserRoleIdFromApi(currentUserId);
            try {
                sessionStorage.setItem('UserRolId', String(apiRoleId));
                sessionStorage.setItem('UserRoleId', String(apiRoleId));
            } catch { }

            console.log('[Login] Kullanıcının RoleId değeri (UserRolesApi):', apiRoleId);

            // 3) Kontör bilgisini çek ve session'a yaz
            await fetchUserCreditsFromApi(currentUserId);

            // 4) Firma listesi
            const byFirma = await FirmaApi.list();
            console.log('Firma liste sonucu:', byFirma);
            storeFirmaToSession(res, byFirma);

            // 5) Mükellef yenileme isteğini backend'e at (arka planda)
            await triggerMukellefRefreshOnServer(currentUserId);

            // 6) Son e-posta'yı sakla
            try { sessionStorage.setItem('lastLoginEmail', email); } catch { }

            // 7) Artık home'a yönlen
            window.location.href = homeUrl;
        } catch (err) {
            console.error('Login error:', err);
            showError(err?.message || 'Beklenmeyen bir hata oluştu.');
        } finally {
            // Başarılı login'de redirect hemen gideceği için
            // buradaki kapama çoğu zaman görünmeden sayfa değişir,
            // ama hata durumunda overlay kapanmış olur.
            setLoadingState(submitBtn, false);
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

    const userId = getUserIdFlexible(user);
    if (!userId) {
        console.warn('storeFirmaToSession: userId alınamadı. user:', user);
        return;
    }

    const firma = Array.isArray(firmaList)
        ? firmaList.find(f => {
            const fid =
                f.userId ??
                f.UserId ??
                f.KullaniciId ??
                f.kullaniciId ??
                null;
            return fid === userId;
        })
        : null;

    if (firma) {
        sessionStorage.setItem('Firma', JSON.stringify(firma));
        console.log('Firma sessiona yazıldı:', firma);
    } else {
        console.warn('storeFirmaToSession: userId için firma bulunamadı:', userId);
    }
}
