// Auth iş mantığı (token yok) – Kullanıcı listesinden eposta/şifre eşleştir.
import * as KullaniciApi from '../entities/Kullanici.js';

const AUTH_KEY = 'ts.auth';
const isNilOrEmpty = v => v == null || (typeof v === 'string' && v.trim() === '');
const normEmail = e => (e || '').trim().toLowerCase();

function deriveContext(user) {
  const role = (user?.Rol || '').trim();
  if (/^admin$/i.test(role)) return { type: 'Admin' };
  const bayi = user?.BayiBaglantilari || [];
  if (bayi.length) return { type: 'Bayi', bayiId: bayi.find(x => x.IsPrimary)?.BayiId || bayi[0]?.BayiId || null };
  const mm = user?.MaliMusavirBaglantilari || [];
  if (mm.length) return { type: 'MaliMusavir', maliMusavirId: mm.find(x => x.IsPrimary)?.MaliMusavirId || mm[0]?.MaliMusavirId || null };
  const f = user?.FirmaBaglantilari || [];
  if (f.length) return { type: 'Firma', firmaId: f.find(x => x.IsPrimary)?.FirmaId || f[0]?.FirmaId || null };
  return { type: 'Unknown' };
}

export async function signIn(email, password) {
  if (isNilOrEmpty(email) || isNilOrEmpty(password))
    return { success: false, message: 'E-posta ve şifre zorunludur.' };

  const users = await KullaniciApi.list();
  const found = (users || []).find(u => normEmail(u?.Eposta) === normEmail(email));
  if (!found) return { success: false, message: 'Kullanıcı bulunamadı.' };
  if ((found?.Sifre || '') !== password) return { success: false, message: 'Şifre hatalı.' };

  const session = {
    userId: found.Id, adSoyad: found.AdSoyad, eposta: found.Eposta,
    role: found.Rol, context: deriveContext(found), loginAt: new Date().toISOString()
  };
  sessionStorage.setItem(AUTH_KEY, JSON.stringify(session));
  return { success: true, user: found };
}

export function getSession() {
  try { const raw = sessionStorage.getItem(AUTH_KEY); return raw ? JSON.parse(raw) : null; }
  catch { return null; }
}
export function signOut() { sessionStorage.removeItem(AUTH_KEY); }
