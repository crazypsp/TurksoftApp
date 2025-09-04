// Luca CRUD (sade kural: userId -> MMId bul, tabloyu client-side MMId'a göre filtrele, create'te dto'ya MMId set et)
import { LucaApi, KullaniciMaliMusavirApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';

const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

// ---- oturum & MMId
let session = {};
let ctx = {};
let userId = '';
let myMaliMusavirId = ''; // TEK KAYNAK: burada tutulacak

async function resolveSession() {
  try {
    const maybe = getSession?.() ?? {};
    return (maybe && typeof maybe.then === 'function') ? (await maybe || {}) : (maybe || {});
  } catch {
    return {};
  }
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
    // sunucu parametreyi yoksayarsa bile client-side kesin filtre uygulayacağız
    rows = await KullaniciMaliMusavirApi.list({ kullaniciId: String(uid) });
  } catch {
    rows = await KullaniciMaliMusavirApi.list();
  }

  const mine = (Array.isArray(rows) ? rows : [])
    .filter(r => String(r.KullaniciId ?? r.kullaniciId ?? '') === String(uid));

  if (!mine.length) return;

  const primary = mine.find(r => r.IsPrimary === true);
  myMaliMusavirId = String((primary ?? mine[0]).MaliMusavirId ?? (primary ?? mine[0]).maliMusavirId ?? '');
}

// ------------------------------------------------------
document.addEventListener('DOMContentLoaded', async () => {
  // oturum ve MMId'yi yükle
  applySession(await resolveSession());
  await resolveMyMaliMusavirId(userId);

  const tbody = $('#tbl-luca-body');
  const fUye = $('#fltUyeNo'), fUser = $('#fltKullaniciAdi');
  const btnNew = $('#btnNewLuca');
  const modalEl = $('#mdlLuca'); const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmLuca');
  const fmId = $('#frmId'), fmUye = $('#frmUyeNo'), fmUser = $('#frmKullaniciAdi'), fmPwd = $('#frmParola');

  async function loadTable() {
    // her zaman tüm listeyi çek
    const list = await LucaApi.list();

    // sadece user'ın MMId'si ile eşleşen kayıtlar
    const byMm = (Array.isArray(list) ? list : []).filter(x =>
      myMaliMusavirId && String(x.MaliMusavirId ?? x.maliMusavirId ?? '') === String(myMaliMusavirId)
    );

    // ek ekran filtreleri
    const qU = val(fUye).toLowerCase(), qK = val(fUser).toLowerCase();
    const filtered = byMm.filter(x => {
      const okU = !qU || (x.UyeNo || '').toLowerCase().includes(qU);
      const okK = !qK || (x.KullaniciAdi || '').toLowerCase().includes(qK);
      return okU && okK;
    });

    tbody.innerHTML = filtered.map(r => `
      <tr>
        <td>${r.UyeNo || ''}</td>
        <td>${r.KullaniciAdi || ''}</td>
        <td>••••••••</td>
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
      const r = await LucaApi.get(id);
      // güvenlik: sadece kendi MM kaydını zaten listeledik; yine de kontrol etmek istersen:
      // if (String(r.MaliMusavirId ?? r.maliMusavirId ?? '') !== myMaliMusavirId) return alert('Yetki yok');

      fmId.value = r.Id; fmUye.value = r.UyeNo || ''; fmUser.value = r.KullaniciAdi || ''; fmPwd.value = r.Parola || '';
      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await LucaApi.remove(id); await loadTable();
    }));
  }

  btnNew?.addEventListener('click', () => { formEl.reset(); fmId.value = ''; modal?.show(); });

  formEl?.addEventListener('submit', async e => {
    e.preventDefault();

    if (!myMaliMusavirId) { alert('Mali Müşavir eşleşmesi bulunamadı.'); return; }

    const dto = {
      Id: fmId.value || undefined,
      UyeNo: fmUye.value,
      KullaniciAdi: fmUser.value,
      Parola: fmPwd.value,
      // CRUX: her CREATE/UPDATE gönderiminde MMId sabit
      MaliMusavirId: myMaliMusavirId
    };

    if (dto.Id) await LucaApi.update(dto.Id, dto);
    else await LucaApi.create(dto);

    modal?.hide(); await loadTable();
  });

  [fUye, fUser].forEach(el => el?.addEventListener('input', loadTable));

  await loadTable();
});
