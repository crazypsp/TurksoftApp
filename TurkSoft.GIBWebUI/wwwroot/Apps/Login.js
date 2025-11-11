// /apps/login-glue.js
import { signIn, getSession } from '../Service/Login.js';

function ensureErrorBox() {
    let el = document.getElementById('loginError');
    if (!el) {
        el = document.createElement('div');
        el.id = 'loginError';
        el.className = 'alert alert-danger';
        el.style.display = 'none';
        const form = document.getElementById('loginForm');
        form?.prepend(el);
    }
    return el;
}

function isValidEmail(email) { return /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/.test(email); }
function setInvalid($input, message) {
    if (!$input) return;
    $input.classList.add('is-invalid');
    let fb = $input.nextElementSibling && $input.nextElementSibling.classList?.contains('invalid-feedback')
        ? $input.nextElementSibling : null;
    if (!fb) {
        fb = document.createElement('div');
        fb.className = 'invalid-feedback';
        $input.insertAdjacentElement('afterend', fb);
    }
    fb.textContent = message || '';
}
function clearInvalid($input) { if ($input) $input.classList.remove('is-invalid'); }
function wireValidation(emailInput, passInput) {
    const clear = (e) => { e.target.classList.remove('is-invalid'); };
    emailInput?.addEventListener('input', clear);
    passInput?.addEventListener('input', clear);
}
function showError(msg) { const box = ensureErrorBox(); box.textContent = msg || ''; box.style.display = msg ? 'block' : 'none'; }

document.addEventListener('DOMContentLoaded', async () => {
    const form = document.getElementById('loginForm');
    const emailInput = document.getElementById('Email');
    const passwordInput = document.getElementById('Password');
    const submitBtn = form?.querySelector('button[type="submit"]');
    const homeUrl = form?.getAttribute('data-home-url') || '/';

    wireValidation(emailInput, passwordInput);
    showError('');

    try {
        const existing = await getSession({ hydrate: true });
        if (existing?.userId) { window.location.href = homeUrl; return; }
    } catch { }

    form?.addEventListener('submit', async (e) => {
        e.preventDefault();
        showError('');

        const email = (emailInput?.value || '').trim();
        const pass = (passwordInput?.value || '').trim();

        let ok = true;
        if (!email) { setInvalid(emailInput, 'E-posta zorunludur.'); ok = false; }
        else if (!isValidEmail(email)) { setInvalid(emailInput, 'Geçerli bir e-posta adresi girin.'); ok = false; }
        else { clearInvalid(emailInput); }

        if (!pass) { setInvalid(passwordInput, 'Şifre zorunludur.'); ok = false; }
        else { clearInvalid(passwordInput); }

        if (!ok) return;

        submitBtn?.setAttribute('disabled', 'disabled');
        submitBtn?.classList.add('disabled');

        try {
            const res = await signIn(email, pass);            
            if (!res.success) { showError(res.message || 'Giriş başarısız.'); return; }
            sessionStorage.setItem('lastLoginEmail', email);
            window.location.href = homeUrl;
        } catch (err) {
            sessionStorage.setItem('lastLoginEmail', email);
            window.location.href = homeUrl;
            //showError(err?.message || 'Beklenmeyen bir hata.');
        } finally {
            sessionStorage.setItem('lastLoginEmail', email);
            window.location.href = homeUrl;
            //submitBtn?.removeAttribute('disabled');
            //submitBtn?.classList.remove('disabled');
        }
    });
});
