// /apps/login-glue.js
import { signIn, getSession, clearAllSessions } from '../Service/Login.js';

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
            const res = await signIn(email, pass);

            if (!res.success) {
                showError(res.message || 'Giriş başarısız.');
                return;
            }

            try { sessionStorage.setItem('lastLoginEmail', email); } catch { }
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
