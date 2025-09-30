import { OpportunityApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-opportunity-body');
  const btnNew = $('#btnNewOpportunity');
  const modalEl = $('#mdlOpportunity');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmOpportunity');

  // Form alanları
  const fmId = $('#frmId');
  const fmFirsatNo = $('#frmFirsatNo');
  const fmBayiId = $('#frmBayiId');
  const fmMaliMusavirId = $('#frmMaliMusavirId');
  const fmFirmaId = $('#frmFirmaId');
  const fmAsamaId = $('#frmAsamaId');
  const fmTahminiTutar = $('#frmTahminiTutar');
  const fmDurum = $('#frmDurum');
  const fmOlusturmaTarihi = $('#frmOlusturmaTarihi');

  // ---- Log helper ----
  async function logIslem(tip) {
    try {
      const session = await getSession();
      const userId = session?.Id || "0";
      const ipAdres = await fetch("https://api.ipify.org?format=json")
        .then(r => r.json())
        .then(d => d.ip)
        .catch(() => "unknown");

      const dto = {
        Islem: `${userId}-${tip}`,
        IpAdres: ipAdres,
        Tarayici: navigator.userAgent
      };
      await LogApi.create(dto);
    } catch (err) {
      console.error("Log yazılamadı:", err);
    }
  }

  // ---- Listeleme ----
  async function loadTable() {
    const list = await OpportunityApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.FirsatNo ?? ''}</td>
        <td>${r.TahminiTutar ?? ''}</td>
        <td>${r.Durum ?? ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del"  data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await OpportunityApi.get(id);
    await logIslem("GetById");
    return r;
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await getById(id);

      fmId.value = r.Id;
      fmFirsatNo.value = r.FirsatNo ?? '';
      fmBayiId.value = r.BayiId ?? '';
      fmMaliMusavirId.value = r.MaliMusavirId ?? '';
      fmFirmaId.value = r.FirmaId ?? '';
      fmAsamaId.value = r.AsamaId ?? '';
      fmTahminiTutar.value = r.TahminiTutar ?? '';
      fmDurum.value = r.Durum ?? 0;
      fmOlusturmaTarihi.value = r.OlusturmaTarihi ?? '';

      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await OpportunityApi.remove(id);
      await logIslem("Delete");
      await loadTable();
    }));
  }

  // ---- Yeni kayıt ----
  btnNew?.addEventListener('click', () => {
    formEl.reset();
    fmId.value = '';
    modal?.show();
  });

  // ---- Insert/Update ----
  formEl?.addEventListener('submit', async e => {
    e.preventDefault();
    const dto = {
      Id: fmId.value || undefined,
      FirsatNo: val(fmFirsatNo),
      BayiId: val(fmBayiId),
      MaliMusavirId: val(fmMaliMusavirId) || null,
      FirmaId: val(fmFirmaId) || null,
      AsamaId: val(fmAsamaId),
      TahminiTutar: parseFloat(val(fmTahminiTutar)) || 0,
      Durum: parseInt(val(fmDurum)) || 0,
      OlusturmaTarihi: val(fmOlusturmaTarihi)
    };

    if (dto.Id) {
      await OpportunityApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await OpportunityApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
