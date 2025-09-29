import { OpportunityAsamaGecisApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-opportunityasamagecis-body');
  const btnNew = $('#btnNewOpportunityAsamaGecis');
  const modalEl = $('#mdlOpportunityAsamaGecis');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmOpportunityAsamaGecis');

  // Form alanları
  const fmId = $('#frmId');
  const fmOpportunityId = $('#frmOpportunityId');
  const fmFromAsamaId = $('#frmFromAsamaId');
  const fmToAsamaId = $('#frmToAsamaId');
  const fmGecisTarihi = $('#frmGecisTarihi');

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
    const list = await OpportunityAsamaGecisApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.OpportunityId ?? ''}</td>
        <td>${r.FromAsamaId ?? ''}</td>
        <td>${r.ToAsamaId ?? ''}</td>
        <td>${r.GecisTarihi ? new Date(r.GecisTarihi).toLocaleDateString() : ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del"  data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await OpportunityAsamaGecisApi.get(id);
    await logIslem("GetById");
    return r;
  }

  // ---- Satır aksiyonları ----
  function bindRowActions() {
    $$('.act-edit').forEach(b =>
      b.addEventListener('click', async e => {
        const id = e.currentTarget.getAttribute('data-id');
        const r = await getById(id);

        fmId.value = r.Id;
        fmOpportunityId.value = r.OpportunityId ?? '';
        fmFromAsamaId.value = r.FromAsamaId ?? '';
        fmToAsamaId.value = r.ToAsamaId ?? '';
        fmGecisTarihi.value = r.GecisTarihi ? new Date(r.GecisTarihi).toISOString().substring(0, 10) : '';

        modal?.show();
      })
    );
    $$('.act-del').forEach(b =>
      b.addEventListener('click', async e => {
        const id = e.currentTarget.getAttribute('data-id');
        if (!confirm('Silinsin mi?')) return;
        await OpportunityAsamaGecisApi.remove(id);
        await logIslem("Delete");
        await loadTable();
      })
    );
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
      OpportunityId: val(fmOpportunityId),
      FromAsamaId: val(fmFromAsamaId),
      ToAsamaId: val(fmToAsamaId),
      GecisTarihi: fmGecisTarihi.value ? new Date(fmGecisTarihi.value).toISOString() : null
    };

    if (dto.Id) {
      await OpportunityAsamaGecisApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await OpportunityAsamaGecisApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
