// wwwroot/ApiJS/Guards/menu-guard-ids.js
import { getSession } from '../Service/LoginService.js';

const ALL_IDS = [
  'menuGrpMuhasebe',
  'menuMM',
  'menuKullanici',
  'menuTransfers',
  'menuLuca',
  'menuKeyAccount'
];

const MAP = {
  // admin: hepsi
  admin: ALL_IDS,
  // bayi: sadece MM ve Kullanıcı + grup
  bayi: ['menuGrpMuhasebe', 'menuMM', 'menuKullanici'],
  // mali müşavir: sadece Transfers/Luca/KeyAccount + grup
  malimusavir: ['menuGrpMuhasebe', 'menuTransfers', 'menuLuca', 'menuKeyAccount']
};

const low = s => (s ?? '').toString().trim().toLowerCase()
  .replace(/ı/g, 'i').replace(/ğ/g, 'g').replace(/ü/g, 'u')
  .replace(/ş/g, 's').replace(/ö/g, 'o').replace(/ç/g, 'c');

function hideAll() {
  ALL_IDS.forEach(id => {
    const el = document.getElementById(id);
    if (el) el.classList.add('d-none'); // Bootstrap var
  });
}

function showIds(ids = []) {
  ids.forEach(id => {
    const el = document.getElementById(id);
    if (el) el.classList.remove('d-none');
  });

  // grupta görünür çocuk yoksa grubu kapat
  const grp = document.getElementById('menuGrpMuhasebe');
  if (grp) {
    const childIds = ['menuMM', 'menuKullanici', 'menuTransfers', 'menuLuca', 'menuKeyAccount'];
    const any = childIds.some(cid => {
      const c = document.getElementById(cid);
      return c && !c.classList.contains('d-none');
    });
    grp.classList.toggle('d-none', !any);
  }
}

async function resolveSession() {
  try {
    const maybe = getSession?.() ?? {};
    return (maybe && typeof maybe.then === 'function') ? (await maybe || {}) : (maybe || {});
  } catch { return {}; }
}

document.addEventListener('DOMContentLoaded', async () => {
  const s = await resolveSession();
  const roleKey = low(s.role || s.Role || s?.context?.type || '');

  // default: rol çözülemezse hiçbir şeyi gizlemeyelim (fail-open)
  if (!roleKey || !MAP[roleKey]) return;

  hideAll();
  showIds(MAP[roleKey]);
});
