

import { UserApi, UserRolesApi, RoleApi } from '../Entites/index.js';


async function getUserByEmail(email) {
    const norm = (email || '').trim().toLowerCase();
    if (!norm) return null;

    // 1) ?email= parametresi destekleniyorsa deneyin
    try {
        const byQuery = await UserApi.list({ email: norm });
        if (Array.isArray(byQuery) && byQuery.length)
            return byQuery.find(x => ((x.Email || x.Eposta || '').trim().toLowerCase() === norm)) || byQuery[0];
        if (byQuery && !Array.isArray(byQuery)) {
            const hit = (byQuery.Email || byQuery.Eposta || '').trim().toLowerCase() === norm;
            if (hit) return byQuery;
        }
    } catch { /* fallback */ }

    // 2) Liste çekip client tarafı filtre
    const list = await UserApi.list();
    if (!Array.isArray(list)) return null;
    return list.find(x => ((x.Email || x.Eposta || '').trim().toLowerCase() === norm)) || null;
}

async function getUserRoles(userId) {
    if (!userId) return [];
    // 1) ?userId=
    try {
        const res = await UserRolesApi.list({ userId });
        if (Array.isArray(res)) return res;
    } catch { /* fallback */ }
    // 2) liste + filtre
    const all = await UserRolesApi.list();
    return (all || []).filter(x => x.UserId === userId);
}

async function getRolesMap() {
    const list = await RoleApi.list();
    const map = new Map();
    (Array.isArray(list) ? list : []).forEach(r => map.set(r.Id, (r.Name || r.Rol || '').trim()));
    return map; // Map<RoleId, RoleName>
}

// --- PBKDF2/WebCrypto yardımcıları ---
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
function textToBytes(s) { return new TextEncoder().encode(s); }

async function pbkdf2(password, saltBytes, iterations, outLenBytes) {
    if (!('crypto' in window) || !window.crypto.subtle)
        throw new Error('Tarayıcı PBKDF2 (Web Crypto) desteklemiyor.');
    const key = await crypto.subtle.importKey('raw', textToBytes(password), { name: 'PBKDF2' }, false, ['deriveBits']);
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
    if (typeof stored !== 'string' || !stored.startsWith('PBKDF2$')) throw new Error('Geçersiz hash.');
    const parts = stored.split('$');
    if (parts.length !== 4) throw new Error('Geçersiz PBKDF2 formatı.');
    const iterations = parseInt(parts[1], 10);
    if (!Number.isFinite(iterations) || iterations <= 0) throw new Error('Geçersiz iteration.');
    const salt = b64ToBytes(parts[2]);
    const hash = b64ToBytes(parts[3]);
    return { iterations, salt, hash, hashLen: hash.length };
}

async function verifyPassword(password, storedHash) {
    const { iterations, salt, hash, hashLen } = parsePBKDF2(storedHash);
    const derived = await pbkdf2(password, salt, iterations, hashLen);
    return constantTimeEqual(derived, hash);
}

// --- Context türetme ---
function deriveContextFromRoleNames(roleNames = []) {
    const set = new Set(roleNames.map(r => (r || '').toLowerCase()));
    if (set.has('admin')) return { type: 'Admin' };
    if (set.has('bayi')) return { type: 'Bayi' };
    if (set.has('malimüşavir') || set.has('malimusavir') || set.has('malimüsavir')) return { type: 'MaliMüşavir' };
    if (set.has('firma')) return { type: 'Firma' };
    return { type: 'Unknown' };
}

// --- Session (ss + ls + meta) ---
const STORAGE = { ssKey: 'ts.auth', lsKey: 'ts.auth.backup', metaName: 'ts-session' };

function _persistSession(sessionObj) {
    const raw = JSON.stringify(sessionObj);
    try { sessionStorage.setItem(STORAGE.ssKey, raw); } catch { }
    try { localStorage.setItem(STORAGE.lsKey, raw); } catch { }
}
function _readSessionRaw() {
    try { const s = sessionStorage.getItem(STORAGE.ssKey); if (s) return JSON.parse(s); } catch { }
    try {
        const s = localStorage.getItem(STORAGE.lsKey);
        if (s) { try { sessionStorage.setItem(STORAGE.ssKey, s); } catch { } return JSON.parse(s); }
    } catch { }
    const meta = document.querySelector(`meta[name="${STORAGE.metaName}"]`);
    if (meta && meta.content) { try { const s = JSON.parse(meta.content); _persistSession(s); return s; } catch { } }
    return null;
}

export async function getSession(opts = { hydrate: false }) {
    const { hydrate = false } = (opts || {});
    let session = _readSessionRaw();
    if (!session) return null;
    if (!hydrate) return session;
    if (session.context && session.context.type) return session;

    if (Array.isArray(session.roles) && session.roles.length) {
        session = { ...session, context: deriveContextFromRoleNames(session.roles) };
        _persistSession(session);
        return session;
    }

    try {
        if (!session.userId) return session;
        const userRoles = await getUserRoles(session.userId);
        const roleMap = await getRolesMap();
        const roleNames = (userRoles || [])
            .filter(x => x.IsActive !== false)
            .map(x => roleMap.get(x.RoleId))
            .filter(Boolean);
        const ctx = deriveContextFromRoleNames(roleNames);
        session = { ...session, roles: roleNames, context: ctx };
        _persistSession(session);
    } catch { }
    return session;
}

export function setSession(sessionObj) { if (!sessionObj) return signOut(); _persistSession(sessionObj); }
export function signOut() {
    try { sessionStorage.removeItem(STORAGE.ssKey); } catch { }
    try { localStorage.removeItem(STORAGE.lsKey); } catch { }
}

// --- SignIn ---
export async function signIn(email, password) {
    const normEmail = (email || '').trim().toLowerCase();
    if (!normEmail || !password) return { success: false, message: 'E-posta ve şifre zorunludur.' };

    const user = await getUserByEmail(normEmail);
    if (!user) return { success: false, message: 'Kullanıcı bulunamadı.' };
    if (user.IsActive === false) return { success: false, message: 'Kullanıcı pasif.' };

    //const storedHash = user.PasswordHash || user.Password || user.Sifre;
    //if (!storedHash || !String(storedHash).startsWith('PBKDF2$'))
    //    return { success: false, message: 'Sunucudaki parola formatı desteklenmiyor.' };

    //const ok = await verifyPassword(password, storedHash);
    //if (!ok) return { success: false, message: 'Şifre hatalı.' };

    const userRoles = await getUserRoles(user.Id);
    const roleMap = await getRolesMap();
    const roleNames = (userRoles || [])
        .filter(x => x.IsActive !== false)
        .map(x => roleMap.get(x.RoleId))
        .filter(Boolean);

    const context = deriveContextFromRoleNames(roleNames);

    const session = {
        userId: user.Id,
        adSoyad: user.FullName || user.AdSoyad || user.Username || '',
        eposta: user.Email || user.Eposta || '',
        roles: roleNames,
        context,
        loginAt: new Date().toISOString()
    };
    _persistSession(session);
    return { success: true, user, roles: roleNames, context };
}

// İsteğe bağlı
export const _internals = { parsePBKDF2, verifyPassword, pbkdf2, b64ToBytes, bytesToB64 };
export { deriveContextFromRoleNames as deriveContext };
