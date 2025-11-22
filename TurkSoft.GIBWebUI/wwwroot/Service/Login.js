// /Service/Login.js
import { UserApi, UserRolesApi, RoleApi } from '../Entites/index.js';

/* ----------------------------------------------------
 *  KÜÇÜK YARDIMCILAR
 * --------------------------------------------------*/
function normalizeEmail(val) {
    return (val || '').trim().toLowerCase();
}

// Kullanıcıdan e-posta okunurken tüm olası property isimlerini dene
function getUserEmailRaw(u) {
    if (!u) return '';
    return (
        u.Email ??
        u.email ??
        u.Eposta ??
        u.eposta ??
        u.EPosta ??
        u.ePosta ??
        u.Username ??
        u.username ??
        ''
    );
}
function getUserEmailForCompare(u) {
    return normalizeEmail(getUserEmailRaw(u));
}

function getUserId(u) {
    if (!u) return null;
    return u.Id ?? u.id ?? u.UserId ?? u.userId ?? null;
}

function getUserIsActive(u) {
    if (!u) return true;
    const v = u.IsActive ?? u.isActive ?? u.Active ?? u.active;
    return v === undefined ? true : !!v;
}

function getUserPasswordStored(u) {
    if (!u) return null;
    return (
        u.PasswordHash ??
        u.passwordHash ??
        u.Password ??
        u.password ??
        u.Sifre ??
        u.sifre ??
        null
    );
}

function getUserDisplayName(u) {
    if (!u) return '';
    return (
        u.FullName ??
        u.fullName ??
        u.AdSoyad ??
        u.adSoyad ??
        u.DisplayName ??
        u.displayName ??
        u.Username ??
        u.username ??
        ''
    );
}

/* ----------------------------------------------------
 *  KULLANICI / ROL SORGULARI
 * --------------------------------------------------*/
async function getUserByEmail(email) {
    const norm = normalizeEmail(email);
    if (!norm) return null;

    let candidate = null;

    // 1) API filtre parametresini destekliyorsa buradan deneriz
    try {
        const byQuery = await UserApi.list({ email: norm });
        const arr = Array.isArray(byQuery) ? byQuery : (byQuery ? [byQuery] : []);
        candidate = arr.find(u => getUserEmailForCompare(u) === norm) || null;

        if (candidate) {
            // console.debug('[Login] Kullanıcı email filtresinden bulundu:', candidate);
            return candidate;
        }
    } catch (err) {
        console.warn('[Login] UserApi.list({ email }) çağrısı başarısız:', err);
    }

    // 2) Tüm listeyi çekip client tarafı filtre
    try {
        const list = await UserApi.list();
        const arr = Array.isArray(list) ? list : (list ? [list] : []);
        candidate = arr.find(u => getUserEmailForCompare(u) === norm) || null;

        if (candidate) {
            // console.debug('[Login] Kullanıcı full listeden bulundu:', candidate);
            return candidate;
        }
    } catch (err) {
        console.warn('[Login] UserApi.list() çağrısı başarısız:', err);
    }

    // 3) ÖZEL TEST KULLANICISI (backend'de olmasa bile)
    //    E-posta: firma@gib.com
    //    Şifre : Firma!123
    if (norm === 'firma@gib.com') {
        console.warn('[Login] API içinde firma@gib.com bulunamadı; test kullanıcısı kullanılıyor.');
        return {
            Id: 9999,
            FullName: 'Test Firma Kullanıcısı',
            Email: 'firma@gib.com',
            IsActive: true,
            // Şifreyi plain text saklıyoruz, verifyPasswordFlexible bunu kabul ediyor.
            Password: 'Firma!123'
        };
    }

    return null;
}

// UserRoles için UserId / userId vs.
function getUserRoleUserId(x) {
    if (!x) return null;
    return x.UserId ?? x.userId ?? x.KullaniciId ?? x.kullaniciId ?? null;
}

async function getUserRoles(userId) {
    if (!userId) return [];

    // 1) ?userId= destekleniyorsa
    try {
        const res = await UserRolesApi.list({ userId });
        if (Array.isArray(res)) return res;
    } catch { }

    // 2) Liste + filtre
    try {
        const all = await UserRolesApi.list();
        return (all || []).filter(x => getUserRoleUserId(x) === userId);
    } catch {
        return [];
    }
}

// Role için Id / id / RoleId vs.
async function getRolesMap() {
    try {
        const list = await RoleApi.list();
        const map = new Map();

        (Array.isArray(list) ? list : []).forEach(r => {
            const id = r.Id ?? r.id ?? r.RoleId ?? r.roleId;
            if (id == null) return;
            const name = (r.Name || r.Rol || r.name || r.rol || '').trim();
            map.set(id, name);
        });

        return map; // Map<RoleId, RoleName>
    } catch {
        return new Map();
    }
}

/* ----------------------------------------------------
 *  PBKDF2 / WebCrypto Yardımcıları
 * --------------------------------------------------*/
function b64ToBytes(b64) {
    const bin = atob(b64);
    const out = new Uint8Array(bin.length);
    for (let i = 0; i < bin.length; i++) out[i] = bin.charCodeAt(i);
    return out;
}

function bytesToB64(bytes) {
    let s = '';
    for (let i = 0; i < bytes.length; i++) s += String.fromCharCode(bytes[i]);
    return btoa(s);
}

function textToBytes(s) {
    return new TextEncoder().encode(s);
}

async function pbkdf2(password, saltBytes, iterations, outLenBytes) {
    if (!('crypto' in window) || !window.crypto.subtle) {
        throw new Error('Tarayıcı PBKDF2 (Web Crypto) desteklemiyor.');
    }

    const key = await crypto.subtle.importKey(
        'raw',
        textToBytes(password),
        { name: 'PBKDF2' },
        false,
        ['deriveBits']
    );

    const bits = await crypto.subtle.deriveBits(
        { name: 'PBKDF2', salt: saltBytes, iterations, hash: 'SHA-256' },
        key,
        outLenBytes * 8
    );

    return new Uint8Array(bits);
}

function constantTimeEqual(a, b) {
    if (!a || !b || a.length !== b.length) return false;
    let diff = 0;
    for (let i = 0; i < a.length; i++) diff |= (a[i] ^ b[i]);
    return diff === 0;
}

// Hash formatı: PBKDF2$<iter>$<salt>$<hash>
function parsePBKDF2(stored) {
    if (typeof stored !== 'string' || !stored.startsWith('PBKDF2$')) {
        throw new Error('Geçersiz hash.');
    }
    const parts = stored.split('$');
    if (parts.length !== 4) throw new Error('Geçersiz PBKDF2 formatı.');

    const iterations = parseInt(parts[1], 10);
    if (!Number.isFinite(iterations) || iterations <= 0) {
        throw new Error('Geçersiz iteration.');
    }

    const salt = b64ToBytes(parts[2]);
    const hash = b64ToBytes(parts[3]);
    return { iterations, salt, hash, hashLen: hash.length };
}

async function verifyPBKDF2Password(password, storedHash) {
    const { iterations, salt, hash, hashLen } = parsePBKDF2(storedHash);
    const derived = await pbkdf2(password, salt, iterations, hashLen);
    return constantTimeEqual(derived, hash);
}

/**
 * Esnek doğrulama:
 *  - stored "PBKDF2$..." ise PBKDF2 ile kontrol
 *  - değilse düz metin karşılaştırma (geliştirme/geçiş süreci için)
 */
async function verifyPasswordFlexible(password, stored) {
    if (!stored) return false;
    const storedStr = String(stored);

    if (storedStr.startsWith('PBKDF2$')) {
        try {
            return await verifyPBKDF2Password(password, storedStr);
        } catch (err) {
            console.error('PBKDF2 verify error:', err);
            return false;
        }
    }

    // Fallback: plain text
    return storedStr === password;
}

/* ----------------------------------------------------
 *  ROL -> CONTEXT
 * --------------------------------------------------*/
function deriveContextFromRoleNames(roleNames = []) {
    const set = new Set(roleNames.map(r => (r || '').toLowerCase()));
    if (set.has('admin')) return { type: 'Admin' };
    if (set.has('bayi')) return { type: 'Bayi' };
    if (set.has('malimüşavir') || set.has('malimusavir') || set.has('malimüsavir')) return { type: 'MaliMüşavir' };
    if (set.has('firma')) return { type: 'Firma' };
    return { type: 'Unknown' };
}

/* ----------------------------------------------------
 *  SESSION (sessionStorage + localStorage + meta)
 * --------------------------------------------------*/
const STORAGE = {
    ssKey: 'ts.auth',
    lsKey: 'ts.auth.backup',
    metaName: 'ts-session'
};

function _persistSession(sessionObj) {
    const raw = JSON.stringify(sessionObj);
    try { sessionStorage.setItem(STORAGE.ssKey, raw); } catch { }
    try { localStorage.setItem(STORAGE.lsKey, raw); } catch { }
}

function _readSessionRaw() {
    // 1) sessionStorage
    try {
        const s = sessionStorage.getItem(STORAGE.ssKey);
        if (s) return JSON.parse(s);
    } catch { }

    // 2) localStorage -> sessionStorage'a geri yaz
    try {
        const s = localStorage.getItem(STORAGE.lsKey);
        if (s) {
            try { sessionStorage.setItem(STORAGE.ssKey, s); } catch { }
            return JSON.parse(s);
        }
    } catch { }

    // 3) <meta name="ts-session" content="...">
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

export async function getSession(opts = { hydrate: false }) {
    const { hydrate = false } = (opts || {});
    let session = _readSessionRaw();
    if (!session) return null;
    if (!hydrate) return session;

    // context zaten varsa tekrar uğraşma
    if (session.context && session.context.type) return session;

    // roller varsa context türet
    if (Array.isArray(session.roles) && session.roles.length) {
        session = {
            ...session,
            context: deriveContextFromRoleNames(session.roles)
        };
        _persistSession(session);
        return session;
    }

    // Roller yoksa sunucudan çekip tamamla
    try {
        if (!session.userId) return session;

        const userRoles = await getUserRoles(session.userId);
        const roleMap = await getRolesMap();
        const roleNames = (userRoles || [])
            .filter(x => x.IsActive !== false && x.isActive !== false)
            .map(x => roleMap.get(x.RoleId ?? x.roleId ?? x.Id ?? x.id))
            .filter(Boolean);

        const ctx = deriveContextFromRoleNames(roleNames);
        session = { ...session, roles: roleNames, context: ctx };
        _persistSession(session);
    } catch {
        // sessiz
    }

    return session;
}

export function setSession(sessionObj) {
    if (!sessionObj) {
        signOut();
        return;
    }
    _persistSession(sessionObj);
}

export function signOut() {
    try { sessionStorage.removeItem(STORAGE.ssKey); } catch { }
    try { localStorage.removeItem(STORAGE.lsKey); } catch { }
}

/* ----------------------------------------------------
 *  SIGN IN
 * --------------------------------------------------*/
export async function signIn(email, password) {
    const normEmail = normalizeEmail(email);
    if (!normEmail || !password) {
        return { success: false, message: 'E-posta ve şifre zorunludur.' };
    }

    const user = await getUserByEmail(normEmail);
    const genericFail = { success: false, message: 'Kullanıcı adı veya şifre hatalı.' };

    if (!user) return genericFail;

    const isActive = getUserIsActive(user);
    if (isActive === false) {
        return { success: false, message: 'Kullanıcı pasif.' };
    }

    const storedHash = getUserPasswordStored(user);
    const ok = await verifyPasswordFlexible(password, storedHash);
    if (!ok) return genericFail;

    const userId = getUserId(user);

    // Roller
    const userRoles = await getUserRoles(userId);
    const roleMap = await getRolesMap();
    const roleNames = (userRoles || [])
        .filter(x => x.IsActive !== false && x.isActive !== false)
        .map(x => roleMap.get(x.RoleId ?? x.roleId ?? x.Id ?? x.id))
        .filter(Boolean);

    const context = deriveContextFromRoleNames(roleNames);

    const session = {
        userId,
        adSoyad: getUserDisplayName(user),
        eposta: getUserEmailRaw(user),
        roles: roleNames,
        context,
        loginAt: new Date().toISOString()
    };

    _persistSession(session);

    return {
        success: true,
        user,
        roles: roleNames,
        context
    };
}

/* ----------------------------------------------------
 *  TEST / DIŞA AKTARILANLAR
 * --------------------------------------------------*/
export const _internals = {
    parsePBKDF2,
    verifyPBKDF2Password,
    pbkdf2,
    b64ToBytes,
    bytesToB64
};

export { deriveContextFromRoleNames as deriveContext };
