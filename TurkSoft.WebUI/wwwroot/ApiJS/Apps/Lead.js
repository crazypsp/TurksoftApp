import { LeadApi, LogApi } from '../entities/index.js';
import { getSession } from '../Service/LoginService.js';
const $ = s => document.querySelector(s);
const $$ = s => Array.from(document.querySelectorAll(s));
const val = el => (el?.value ?? '').trim();

document.addEventListener('DOMContentLoaded', async () => {
  const tbody = $('#tbl-lead-body');
  const btnNew = $('#btnNewLead');
  const modalEl = $('#mdlLead');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const formEl = $('#frmLead');

  // Form alanları
  const fmId = $('#frmId');
  const fmLeadNo = $('#frmLeadNo');
  const fmBayiId = $('#frmBayiId');
  const fmUnvan = $('#frmUnvan');
  const fmKaynak = $('#frmKaynak');
  const fmSorumluKullaniciId = $('#frmSorumluKullaniciId');
  const fmAdresUlke = $('#frmAdresUlke');
  const fmAdresSehir = $('#frmAdresSehir');
  const fmAdresIlce = $('#frmAdresIlce');
  const fmAdresPostaKodu = $('#frmAdresPostaKodu');
  const fmAdresAcikAdres = $('#frmAdresAcikAdres');
  const fmOlusturmaTarihi = $('#frmOlusturmaTarihi');
  const fmNotlar = $('#frmNotlar');

  // ---- Log ekleme helper ----
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
    const list = await LeadApi.list();
    await logIslem("List");
    tbody.innerHTML = (Array.isArray(list) ? list : []).map(r => `
      <tr>
        <td>${r.Id}</td>
        <td>${r.LeadNo ?? ''}</td>
        <td>${r.Unvan ?? ''}</td>
        <td>${r.Kaynak ?? ''}</td>
        <td>${r.OlusturmaTarihi ?? ''}</td>
        <td>${r.Notlar ?? ''}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-primary act-edit" data-id="${r.Id}">Düzenle</button>
          <button class="btn btn-sm btn-danger act-del"  data-id="${r.Id}">Sil</button>
        </td>
      </tr>`).join('');
    bindRowActions();
  }

  // ---- Tek kayıt getirme ----
  async function getById(id) {
    const r = await LeadApi.get(id);
    await logIslem("GetById");
    return r;
  }

  function bindRowActions() {
    $$('.act-edit').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      const r = await getById(id);

      fmId.value = r.Id;
      fmLeadNo.value = r.LeadNo ?? '';
      fmBayiId.value = r.BayiId ?? '';
      fmUnvan.value = r.Unvan ?? '';
      fmKaynak.value = r.Kaynak ?? '';
      fmSorumluKullaniciId.value = r.SorumluKullaniciId ?? '';
      fmAdresUlke.value = r.Adres?.Ulke ?? '';
      fmAdresSehir.value = r.Adres?.Sehir ?? '';
      fmAdresIlce.value = r.Adres?.Ilce ?? '';
      fmAdresPostaKodu.value = r.Adres?.PostaKodu ?? '';
      fmAdresAcikAdres.value = r.Adres?.AcikAdres ?? '';
      fmOlusturmaTarihi.value = r.OlusturmaTarihi ?? '';
      fmNotlar.value = r.Notlar ?? '';

      modal?.show();
    }));
    $$('.act-del').forEach(b => b.addEventListener('click', async e => {
      const id = e.currentTarget.getAttribute('data-id');
      if (!confirm('Silinsin mi?')) return;
      await LeadApi.remove(id);
      await logIslem("Delete");
      await loadTable();
    }));
  }

  // ---- Yeni kayıt butonu ----
  btnNew?.addEventListener('click', () => {
    formEl.reset();
    fmId.value = '';
    modal?.show();
  });

  // ---- Kayıt ekleme / güncelleme ----
  formEl?.addEventListener('submit', async e => {
    e.preventDefault();
    const dto = {
      Id: fmId.value || undefined,
      LeadNo: val(fmLeadNo),
      BayiId: val(fmBayiId),
      Unvan: val(fmUnvan),
      Kaynak: val(fmKaynak),
      SorumluKullaniciId: val(fmSorumluKullaniciId) || null,
      Adres: {
        Ulke: val(fmAdresUlke),
        Sehir: val(fmAdresSehir),
        Ilce: val(fmAdresIlce),
        PostaKodu: val(fmAdresPostaKodu),
        AcikAdres: val(fmAdresAcikAdres)
      },
      OlusturmaTarihi: val(fmOlusturmaTarihi),
      Notlar: val(fmNotlar)
    };

    if (dto.Id) {
      await LeadApi.update(dto.Id, dto);
      await logIslem("Update");
    } else {
      await LeadApi.create(dto);
      await logIslem("Insert");
    }

    modal?.hide();
    await loadTable();
  });

  await loadTable();
});
