import { signIn, getSession } from '../Service/LoginService.js';

document.addEventListener('DOMContentLoaded', () => {
  const form = document.getElementById('loginForm');
  const emailInput = document.getElementById('email');
  const passwordInput = document.getElementById('password');
  const submitBtn = document.getElementById('btnLogin');
  const errorBox = document.getElementById('loginError');

  const showError = (m) => { if (!errorBox) return; errorBox.textContent = m || ''; errorBox.style.display = m ? 'block' : 'none'; };
  const existing = getSession(); if (existing?.userId) { window.location.href = '/Dashboards/Index'; return; }
  showError('');

  form?.addEventListener('submit', async (e) => {
    e.preventDefault();
    submitBtn?.setAttribute('disabled', 'disabled'); submitBtn?.classList.add('disabled');
    try {
      const res = await signIn(emailInput?.value || '', passwordInput?.value || '');
      if (!res.success) { showError(res.message || 'Giriş başarısız.'); return; }
      window.location.href = '/Dashboards/Index';
    } catch (err) { showError(err?.message || 'Beklenmeyen bir hata.'); }
    finally { submitBtn?.removeAttribute('disabled'); submitBtn?.classList.remove('disabled'); }
  });
});
