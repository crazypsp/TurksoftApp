import { turkcellEfaturaApi } from '../Base/turkcellEfaturaApi.js';

(function () {
    "use strict";
    const $ = window.jQuery;
    if (!$) return console.error("[CreateErpEArchiveInvoice] jQuery yok.");

    // ===== Utils =====
    const TR_DATE = {
        closeText: "Kapat", prevText: "Önceki", nextText: "Sonraki", currentText: "Bugün",
        monthNames: ["Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"],
        monthNamesShort: ["Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara"],
        dayNames: ["Pazar", "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi"],
        dayNamesShort: ["Paz", "Pts", "Sal", "Çar", "Per", "Cum", "Cts"],
        dayNamesMin: ["Pz", "Pt", "Sa", "Ça", "Pe", "Cu", "Ct"],
        dateFormat: "dd.mm.yy", firstDay: 1, isRTL: false
    };

    function toApiDateTR(ddmmyyyy) {
        if (!ddmmyyyy) return "";
        const p = ddmmyyyy.split(".");
        if (p.length !== 3) return "";
        return `${p[2]}-${String(p[1]).padStart(2, "0")}-${String(p[0]).padStart(2, "0")}`;
    }

    function notifyOk(m) { window.toastr?.success ? toastr.success(m) : alert(m); }
    function notifyErr(m) { window.toastr?.error ? toastr.error(m) : alert(m); }

    function initDatepicker() {
        if (!$.fn.datepicker) return;
        $.datepicker.setDefaults(TR_DATE);
        $(".datepicker").datepicker({ changeMonth: true, changeYear: true, showAnim: "fadeIn" });

        // start/end min-max
        $("#IssueDateStart").on("change", () => {
            const d = $("#IssueDateStart").datepicker("getDate");
            if (d) $("#IssueDateEnd").datepicker("option", "minDate", d);
        });
        $("#IssueDateEnd").on("change", () => {
            const d = $("#IssueDateEnd").datepicker("getDate");
            if (d) $("#IssueDateStart").datepicker("option", "maxDate", d);
        });
    }

    function initSelect2() {
        if ($.fn.select2) {
            $("#erp_sube, #birimList").select2({ width: "100%" });
        }
    }

    // ===== DOM =====
    const $tbl = $("#grid_erpdenolustur");
    const $branch = $("#erp_sube");
    const $sent = $("#Sent");
    const $appType = $("#AppType");
    const $onekEfatura = $("#erp_onek");
    const $onekEarsiv = $("#erp_onek_earsiv");
    const $onekInternet = $("#erp_onek_earsiv_internet");
    const $gbEtiket = $("#erp_gbetiket");
    const $start = $("#IssueDateStart");
    const $end = $("#IssueDateEnd");

    // Birim eşleme modal alanları
    const $erpBirim = $("#erpBirim");
    const $birimList = $("#birimList");

    let table = null;
    let lastUnitMapContext = null; // { erpUnit: "...", rowId: "..." }

    function fillSelect($sel, items, placeholder) {
        $sel.empty();
        $sel.append(`<option value="">${placeholder || "Seçiniz"}</option>`);
        (items || []).forEach((x) => {
            // x: { value, text }
            $sel.append(`<option value="${String(x.value)}">${String(x.text)}</option>`);
        });
    }

    // ===== Prefix listesi =====
    async function setPrefixErpList() {
        const branchId = $branch.val();
        const appType = $appType.val();

        // Şube seçilmeden boş kalsın
        if (!branchId || branchId === "0") {
            fillSelect($onekEfatura, [], "Seçiniz");
            fillSelect($onekEarsiv, [], "Seçiniz");
            fillSelect($onekInternet, [], "Seçiniz");
            return;
        }

        try {
            const resp = await turkcellEfaturaApi.erpEarchiveCreate.prefixes(Number(branchId), Number(appType || 0));
            // beklenen örnek:
            // { efatura:[{value,text}], earsiv:[...], earsivInternet:[...] }
            fillSelect($onekEfatura, resp?.efatura, "Seçiniz");
            fillSelect($onekEarsiv, resp?.earsiv, "Seçiniz");
            fillSelect($onekInternet, resp?.earsivInternet, "Seçiniz");

            if ($.fn.select2) {
                $onekEfatura.trigger("change");
                $onekEarsiv.trigger("change");
                $onekInternet.trigger("change");
            }
        } catch (e) {
            console.error(e);
            notifyErr(e?.message || "Ön ek listeleri alınamadı.");
        }
    }

    // ===== Filtreler =====
    function buildFilters() {
        return {
            issueDateStart: toApiDateTR($start.val()),
            issueDateEnd: toApiDateTR($end.val()),
            branchId: Number($branch.val() || 0),
            sent: $sent.val(),               // "true" | "false" | ""
            appType: Number($appType.val() || 0),
            prefixEfatura: $onekEfatura.val() || "",
            prefixEarsiv: $onekEarsiv.val() || "",
            prefixEarsivInternet: $onekInternet.val() || "",
            senderAlias: $gbEtiket.val() || ""
        };
    }

    // ===== DataTables =====
    function initTable() {
        if (!$.fn.DataTable) return notifyErr("DataTables bulunamadı.");

        table = $tbl.DataTable({
            serverSide: true,
            processing: true,
            searching: false,
            order: [[7, "desc"]], // Fatura Tarihi (kolon indexine göre ayarla)
            pageLength: 25,
            language: { url: "//cdn.datatables.net/plug-ins/1.13.6/i18n/tr.json" },
            ajax: function (dt, callback) {
                turkcellEfaturaApi.erpEarchiveCreate
                    .search({ dt, filters: buildFilters() })
                    .then((resp) => {
                        // DT response normalize
                        const data = Array.isArray(resp?.data) ? resp.data : [];
                        callback({
                            draw: resp?.draw ?? dt.draw,
                            recordsTotal: resp?.recordsTotal ?? resp?.total ?? 0,
                            recordsFiltered: resp?.recordsFiltered ?? resp?.filtered ?? (resp?.recordsTotal ?? 0),
                            data
                        });
                    })
                    .catch((e) => {
                        console.error(e);
                        notifyErr(e?.message || "Liste alınamadı.");
                        callback({ draw: dt.draw, recordsTotal: 0, recordsFiltered: 0, data: [] });
                    });
            },
            columns: [
                {
                    data: null, orderable: false, searchable: false,
                    render: function (d, t, r) {
                        const id = r?.Id ?? r?.id ?? r?.ErpInvoiceNo ?? r?.erpInvoiceNo ?? "";
                        return `<input type="checkbox" class="rowchk" value="${String(id)}" />`;
                    }
                },
                { data: "ErpInvoiceNo", defaultContent: "" },
                { data: "TargetTitle", defaultContent: "" },
                { data: "TargetIdentifier", defaultContent: "" },
                { data: "TargetAlias", defaultContent: "" },
                { data: "InvoiceType", defaultContent: "" },
                { data: "Scenario", defaultContent: "" },
                {
                    data: "IssueDate",
                    render: (v) => (v ? String(v).substring(0, 10) : "")
                },
                {
                    data: "TotalAmount",
                    className: "text-right",
                    render: (v) => (v == null ? "" : String(v))
                },
                { data: "XsltName", defaultContent: "" },
                { data: "Status", defaultContent: "" },
                {
                    data: null, orderable: false, searchable: false,
                    render: function (d, t, r) {
                        const id = r?.Id ?? r?.id ?? "";
                        const erpUnit = r?.ErpUnit ?? r?.erpUnit ?? "";
                        // İsterseniz burada Preview/Detay vb. ekleyebilirsiniz
                        return `
              <div class="btn-group btn-group-xs">
                <button class="btn btn-warning" title="Birim Eşle"
                        onclick="openUnitMapModal('${String(erpUnit).replace(/'/g, "\\'")}', '${String(id).replace(/'/g, "\\'")}')">
                  <i class="fa fa-exchange"></i>
                </button>
              </div>`;
                    }
                }
            ],
            drawCallback: function () {
                // header select-all reset
                $tbl.find("thead input[type=checkbox]").prop("checked", false);
            }
        });

        // Header select all (tablonun ilk checkbox'ı)
        $tbl.on("change", "thead input[type=checkbox]", function () {
            const checked = this.checked;
            $tbl.find("tbody .rowchk").prop("checked", checked);
        });
    }

    function myFaturaList() {
        if (table) table.ajax.reload();
    }

    function clearSearchInputs() {
        // Tarihleri boşlamak yerine bugünü basmak istersen burada değiştir
        // Şimdilik tüm inputları boşlamayalım; sadece filtre alanlarını resetleyelim
        $branch.val("0").trigger("change");
        $sent.val("false");
        $appType.val("3");
        $onekEfatura.val("");
        $onekEarsiv.val("");
        $onekInternet.val("");

        setPrefixErpList();
        myFaturaList();
    }

    function getSelectedIds() {
        const ids = [];
        $tbl.find("tbody .rowchk:checked").each(function () {
            ids.push(String($(this).val()));
        });
        return ids;
    }

    async function sendAll() {
        const ids = getSelectedIds();
        if (!ids.length) return notifyErr("Lütfen en az bir kayıt seçin.");

        try {
            // örnek dto: backend’e göre genişlet
            const dto = {
                ids,
                senderAlias: $gbEtiket.val() || "",
                branchId: Number($branch.val() || 0),
                appType: Number($appType.val() || 0)
            };
            await turkcellEfaturaApi.erpEarchiveCreate.send(dto);
            notifyOk("Seçilen kayıtlar gönderime alındı.");
            myFaturaList();
        } catch (e) {
            console.error(e);
            notifyErr(e?.message || "Gönderim başarısız.");
        }
    }

    // ===== Birim Eşleme =====
    function openUnitMapModal(erpUnit, rowId) {
        lastUnitMapContext = { erpUnit: erpUnit || "", rowId: rowId || "" };
        $erpBirim.val(lastUnitMapContext.erpUnit);
        $birimList.val("").trigger("change");
        $("#modal-erpBirimEsleme").modal("show");
    }

    async function birimEslemeKayit() {
        if (!lastUnitMapContext?.erpUnit) return notifyErr("ERP birimi bulunamadı.");
        const universalCode = $birimList.val();
        if (!universalCode) return notifyErr("Lütfen evrensel kod seçiniz.");

        try {
            await turkcellEfaturaApi.erpEarchiveCreate.saveUnitMap({
                erpUnit: lastUnitMapContext.erpUnit,
                universalCode,
                rowId: lastUnitMapContext.rowId || null
            });
            notifyOk("Birim eşleme kaydedildi.");
            $("#modal-erpBirimEsleme").modal("hide");
            myFaturaList();
        } catch (e) {
            console.error(e);
            notifyErr(e?.message || "Birim eşleme kaydedilemedi.");
        }
    }

    // ===== Mevcut HTML onclick/onchange ile uyumluluk için globals =====
    window.setPrefixErpList = setPrefixErpList;
    window.myFaturaList = myFaturaList;
    window.clearSearchInputs = clearSearchInputs;
    window.sendAll = sendAll;
    window.birimEslemeKayit = birimEslemeKayit;
    window.openUnitMapModal = openUnitMapModal;

    // ===== Init =====
    function init() {
        initDatepicker();
        initSelect2();
        initTable();

        // şube veya belge tipi değişince prefix yenile
        $branch.on("change", setPrefixErpList);
        $appType.on("change", setPrefixErpList);

        // ilk açılış
        setPrefixErpList();
    }

    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init);
    else init();
})();
