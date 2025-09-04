// KeyAccount CRUD (tek MM sahiplik, strict client-side filter)
import { KeyAccountApi, KullaniciMaliMusavirApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';

const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

// --- oturum & MM kimliği
let session = {};
let ctx = {};
let userId = '';
let myMaliMusavirId = ''; // TEK referans noktası

async function resolveSession() {
  try {
    const maybe = getSession?.() ?? {};
    return (maybe && typeof maybe.then === 'function') ? (await maybe || {}) : (maybe || {});
  } catch { return {}; }
}
function applySession(s) {
  session = s || {};
  ctx = session.context || {};
  userId = String(session.userId || session.UserId || session.Id || session.id || session.KullaniciId || '');
}
async function resolveMyMaliMusavirId(uid) {
  myMaliMusavirId = '';
  if (!uid) return;

  let rows = [];
  try {
    rows = await KullaniciMaliMusavirApi.list({ kullaniciId: String(uid) });
  } catch {
    rows = await KullaniciMaliMusavirApi.list();
  }

  const mine = (Array.isArray(rows) ? rows : [])
    .filter(r => String(r.KullaniciId ?? r.kullaniciId ?? '') === String(uid));

  if (!mine.length) return;
  const primary = mine.find(r => r.IsPrimary === true) || mine[0];
  myMaliMusavirId = String(primary.MaliMusavirId ?? primary.maliMusavirId ?? '');
}

document.addEventListener('DOMContentLoaded', async () => {
  // 1) oturum ve MM kimliğini hazırla
  applySession(await resolveSession());
  await resolveMyMaliMusavirId(userId);

  const tbody = $('#tbl-ka-body');
  const fKod = $('#fltKod'), fAcik = $('#fltAciklama');
  const btnNew = $('#btnNewKA');
  const modalEl = $('#mdlKA'); const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmKA');
  const fmId = $('#frmId'), fmKod = $('#frmKod'), fmAcik = $('#frmAciklama');

  async function loadTable() {
    const list = await KeyAccountApi.list();

    // STRICT: sadece benim MM'ime ait kayıtlar
    if (!myMaliMusavirId) { tbody.innerHTML = ''; return; }

    const onlyMine = (Array.isArray(list) ? list : []).filter(x =>
      String(x.MaliMusavirId ?? x.maliMusavirId ?? '') === myMaliMusavirId
    );

    // ekran filtreleri
    const qK = val(fKod).toLowerCase(), qA = val(fAcik).toLowerCase();
    const filtered = onlyMine.filter(x => {
      const okK = !qK || (x.Kod || '').toLowerCase().includes(qK);
      const okA = !qA || (x.Aciklama || '').toLowerCase().includes(qA);
      return okK && okA;
    });

    tbody.innerHTML = filtered.map(r => `
      <tr>
        <td>${r.Kod || ''}</td>
        <td>${r.Aciklama || ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del"  data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await KeyAccountApi.get(id);

      // güvenlik istersen tekrar doğrula:
      // if (String(r.MaliMusavirId ?? r.maliMusavirId ?? '') !== myMaliMusavirId) return alert('Yetki yok');

      fmId.value = r.Id; fmKod.value = r.Kod || ''; fmAcik.value = r.Aciklama || '';
      modal?.show();
    }));

    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await KeyAccountApi.remove(id);
      await loadTable();
    }));
  }

  btnNew?.addEventListener('click', () => { formEl.reset(); fmId.value = ''; modal?.show(); });

  formEl?.addEventListener('submit', async e => {
    e.preventDefault();

    if (!myMaliMusavirId) { alert('Mali Müşavir eşleşmesi bulunamadı.'); return; }

    const dto = {
      Id: fmId.value || undefined,
      Kod: fmKod.value,
      Aciklama: fmAcik.value,
      // CRUX: MM sahipliği her submitte set
      MaliMusavirId: myMaliMusavirId
    };

    if (dto.Id) await KeyAccountApi.update(dto.Id, dto);
    else await KeyAccountApi.create(dto);

    modal?.hide();
    await loadTable();
  });

  [fKod, fAcik].forEach(el => el?.addEventListener('input', loadTable));
  await loadTable();
});
