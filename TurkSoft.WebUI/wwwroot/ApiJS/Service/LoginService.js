// Auth iş mantığı (token yok) – Kullanıcı listesinden eposta/şifre eşleştir.
// Bu sürümde session saklama/okuma güçlendirildi ve context hydrate edildi.
import * as KullaniciApi from '../entities/Kullanici.js';

const STORAGE = {
  ssKey: 'ts.auth',          // sessionStorage key
  lsKey: 'ts.auth.backup',   // localStorage yedek key
  metaName: 'ts-session'     // <meta name="ts-session" content='{"userId":...}'>
};

const isNilOrEmpty = v => v == null || (typeof v === 'string' && v.trim() === '');
const normEmail = e => (e || '').trim().toLowerCase();

// Kullanıcı nesnesinden bağlam çöz
function deriveContext(user) {
  const role = (user?.Rol || '').trim();
  if (/^admin$/i.test(role)) return { type: 'Admin' };

  const bayi = user?.BayiBaglantilari || user?.KullaniciBayiBaglantilari || [];
  if (Array.isArray(bayi) && bayi.length) {
    const primary = bayi.find(x => x.IsPrimary);
    return { type: 'Bayi', bayiId: (primary?.BayiId || bayi[0]?.BayiId || null) };
  }

  const mm = user?.MaliMusavirBaglantilari || user?.KullaniciMaliMusavirBaglantilari || [];
  if (Array.isArray(mm) && mm.length) {
    const primary = mm.find(x => x.IsPrimary);
    return { type: 'MaliMusavir', maliMusavirId: (primary?.MaliMusavirId || mm[0]?.MaliMusavirId || null) };
  }

  const f = user?.FirmaBaglantilari || user?.KullaniciFirmaBaglantilari || [];
  if (Array.isArray(f) && f.length) {
    const primary = f.find(x => x.IsPrimary);
    return { type: 'Firma', firmaId: (primary?.FirmaId || f[0]?.FirmaId || null) };
  }
  return { type: 'Unknown' };
}

// Dahili: session'ı hem sessionStorage hem localStorage'a yaz
function _persistSession(sessionObj) {
  const raw = JSON.stringify(sessionObj);
  try { sessionStorage.setItem(STORAGE.ssKey, raw); } catch { }
  try { localStorage.setItem(STORAGE.lsKey, raw); } catch { }
}

// Dahili: depolamadan oku (ss → ls → meta)
function _readSessionRaw() {
  // 1) sessionStorage
  try {
    const s = sessionStorage.getItem(STORAGE.ssKey);
    if (s) return JSON.parse(s);
  } catch { }

  // 2) localStorage backup
  try {
    const s = localStorage.getItem(STORAGE.lsKey);
    if (s) {
      // sessionStorage’ı doldur
      try { sessionStorage.setItem(STORAGE.ssKey, s); } catch { }
      return JSON.parse(s);
    }
  } catch { }

  // 3) meta fallback (opsiyonel)
  const meta = document.querySelector(`meta[name="${STORAGE.metaName}"]`);
  if (meta && meta.content) {
    try {
      const s = JSON.parse(meta.content);
      _persistSession(s);
      return s;
    } catch { }
  }

  return null;
}

// PUBLIC: Oturumu hydrate ederek getir (context boşsa API’den tamamlar)
export async function getSession(opts = { hydrate: false }) {
  const { hydrate = false } = (opts || {});
  let session = _readSessionRaw();
  if (!session) return null;

  if (!hydrate) return session;

  // context doluysa direkt dön
  const hasContext = !!(session.context && session.context.type && (session.context.bayiId || session.context.maliMusavirId || session.context.firmaId));
  if (hasContext) return session;

  // context yoksa → kullanıcıyı API’den çek ve context türet
  try {
    if (!session.userId) return session;
    const user = await KullaniciApi.get(session.userId);
    const ctx = deriveContext(user);
    session = { ...session, context: ctx };
    _persistSession(session);
  } catch {
    // API başarısızsa mevcut session aynen döner
  }
  return session;
}

// PUBLIC: Oturumu manuel yazmak istersen
export function setSession(sessionObj) {
  if (!sessionObj) return signOut();
  _persistSession(sessionObj);
}

// AUTH: giriş
export async function signIn(email, password) {
  if (isNilOrEmpty(email) || isNilOrEmpty(password))
    return { success: false, message: 'E-posta ve şifre zorunludur.' };

  const users = await KullaniciApi.list();
  const found = (users || []).find(u => normEmail(u?.Eposta) === normEmail(email));
  if (!found) return { success: false, message: 'Kullanıcı bulunamadı.' };
  if ((found?.Sifre || '') !== password) return { success: false, message: 'Şifre hatalı.' };

  const session = {
    userId: found.Id,
    adSoyad: found.AdSoyad,
    eposta: found.Eposta,
    role: found.Rol,
    context: deriveContext(found),  // giriş anında mevcut bağlara göre
    loginAt: new Date().toISOString()
  };
  _persistSession(session);
  return { success: true, user: found };
}

// AUTH: çıkış
export function signOut() {
  try { sessionStorage.removeItem(STORAGE.ssKey); } catch { }
  try { localStorage.removeItem(STORAGE.lsKey); } catch { }
}
