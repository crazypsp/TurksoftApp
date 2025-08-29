import { MaliMusavirApi, BayiApi } from '../entities/index.js';
const $ = s => document.querySelector(s); const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', () => {
  const tbody = $('#tbl-mm-body');
  const fAd = $('#fltAdSoyad');
  const fEpo = $('#fltEposta');
  const fBayi = $('#fltBayiId');

  const btnNew = $('#btnNewMM');
  const modalEl = $('#mdlMM'); const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmMM');

  const fmId = $('#frmId'), fmAd = $('#frmAdSoyad'), fmTel = $('#frmTelefon'), fmEpo = $('#frmEposta'),
    fmUnv = $('#frmUnvan'), fmVergi = $('#frmVergiNo'), fmTckn = $('#frmTCKN'), fmBayi = $('#frmBayiId');

  async function loadLookups() {
    const bayiler = await BayiApi.list();
    fillSelect(fBayi, bayiler, true);
    fillSelect(fmBayi, bayiler, true);
  }
  function fillSelect(sel, items, addEmpty) {
    if (!sel) return; sel.innerHTML = '';
    if (addEmpty) { const o = document.createElement('option'); o.value = ''; o.textContent = '— Seçiniz —'; sel.appendChild(o); }
    (items || []).forEach(x => { const o = document.createElement('option'); o.value = x.Id; o.textContent = x.Unvan; sel.appendChild(o); });
  }

  async function loadTable() {
    const list = await MaliMusavirApi.list();
    const qAd = val(fAd).toLowerCase(); const qE = val(fEpo).toLowerCase(); const qBayi = val(fBayi);
    const filtered = (list || []).filter(x => {
      const okAd = !qAd || (x.AdSoyad || '').toLowerCase().includes(qAd);
      const okE = !qE || (x.Eposta || '').toLowerCase().includes(qE);
      const okB = !qBayi || x.BayiId === qBayi;
      return okAd && okE && okB;
    });
    tbody.innerHTML = (filtered || []).map(r => `
      <tr>
        <td>${r.AdSoyad || ''}</td>
        <td>${r.Unvan || ''}</td>
        <td>${r.VergiNo || ''}</td>
        <td>${r.TCKN || ''}</td>
        <td>${r.Telefon || ''}</td>
        <td>${r.Eposta || ''}</td>
        <td>${r.Bayi?.Unvan || ''}</td>
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
      const r = await MaliMusavirApi.get(id);
      fmId.value = r.Id; fmAd.value = r.AdSoyad || ''; fmTel.value = r.Telefon || ''; fmEpo.value = r.Eposta || '';
      fmUnv.value = r.Unvan || ''; fmVergi.value = r.VergiNo || ''; fmTckn.value = r.TCKN || ''; fmBayi.value = r.BayiId || '';
      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await MaliMusavirApi.remove(id); await loadTable();
    }));
  }

  btnNew?.addEventListener('click', () => { formEl.reset(); fmId.value = ''; modal?.show(); });

  formEl?.addEventListener('submit', async e => {
    e.preventDefault();
    const dto = {
      Id: fmId.value || undefined, AdSoyad: fmAd.value, Telefon: fmTel.value, Eposta: fmEpo.value,
      Unvan: fmUnv.value, VergiNo: fmVergi.value, TCKN: fmTckn.value,
      BayiId: fmBayi.value || null
    };
    if (dto.Id) await MaliMusavirApi.update(dto.Id, dto); else await MaliMusavirApi.create(dto);
    modal?.hide(); await loadTable();
  });

  [fAd, fEpo, fBayi].forEach(el => { el?.addEventListener('input', loadTable); el?.addEventListener('change', loadTable); });

  (async () => { await loadLookups(); await loadTable(); })();
});
