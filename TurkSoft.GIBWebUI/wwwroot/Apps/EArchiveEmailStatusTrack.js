import { turkcellEfaturaApi } from '../Base/turkcellEfaturaApi.js';

(function () {
    "use strict";

    if (!window.jQuery) {
        console.error("[EArchiveEmailStatusTrack] jQuery bulunamadı.");
        return;
    }
    const $ = window.jQuery;

    const $start = $("#IssueDateStart");
    const $end = $("#IssueDateStartEnd");
    const $btnOk = $("#btnOk");
    const $btnClear = $("#btnClear");
    const $btnRequeue = $("#btnRequeue");
    const $btnExport = $("#btnExport");
    const $chkAll = $("#chkAll");
    const $csvA = $("#csvDownload");
    const $table = $("#myDataTable");

    let grid = null;

    function notifyOk(m) {
        if (window.toastr?.success) toastr.success(m);
        else alert(m);
    }
    function notifyErr(m) {
        if (window.toastr?.error) toastr.error(m);
        else alert(m);
    }

    function html(s) {
        return String(s ?? "")
            .replace(/&/g, "&amp;").replace(/</g, "&lt;")
            .replace(/>/g, "&gt;").replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function yesNo(v) {
        return v === true || v === "YES" || v === 1 ? "Evet" : "Hayır";
    }

    // yyyy-mm-dd doğrulama
    function isValidYMD(s) {
        if (!s) return true;
        if (!/^\d{4}-\d{2}-\d{2}$/.test(s)) return false;
        const d = new Date(s + "T00:00:00");
        if (Number.isNaN(d.getTime())) return false;
        const back = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
        return back === s;
    }

    function validateRange(start, end) {
        if (start && !isValidYMD(start)) return "Başlangıç tarihi geçersiz. Format: yyyy-mm-dd";
        if (end && !isValidYMD(end)) return "Bitiş tarihi geçersiz. Format: yyyy-mm-dd";
        if (start && end && start > end) return "Başlangıç tarihi bitiş tarihinden büyük olamaz.";
        return "";
    }

    function initDatepickers() {
        // jQuery UI datepicker varsa kullan, yoksa input serbest kalsın
        if (!$.fn.datepicker) return;

        const trLocale = {
            closeText: "Kapat", prevText: "‹Geri", nextText: "İleri›",
            currentText: "Bugün",
            monthNames: ["Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"],
            monthNamesShort: ["Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara"],
            dayNames: ["Pazar", "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi"],
            dayNamesShort: ["Pz", "Pt", "Sa", "Ça", "Pe", "Cu", "Ct"],
            dayNamesMin: ["Pz", "Pt", "Sa", "Ça", "Pe", "Cu", "Ct"],
            weekHeader: "Hf",
            dateFormat: "yy-mm-dd",
            firstDay: 1
        };
        $.datepicker.setDefaults(trLocale);

        $(".datepicker").datepicker({
            dateFormat: "yy-mm-dd",
            changeMonth: true,
            changeYear: true,
            showButtonPanel: true,
            onClose: function (selectedDate) {
                if (this.id === "IssueDateStart" && selectedDate) {
                    $end.datepicker("option", "minDate", selectedDate);
                }
                if (this.id === "IssueDateStartEnd" && selectedDate) {
                    $start.datepicker("option", "maxDate", selectedDate);
                }
            }
        });
    }

    function normalizeRow(r) {
        // Backend alanları farklı gelirse tolerans
        const row = r || {};
        return {
            EMailServiceId: row.EMailServiceId ?? row.emailServiceId ?? row.id ?? "",
            FaturaTur: row.FaturaTur ?? row.documentType ?? row.type ?? "",
            UUID: row.UUID ?? row.uuid ?? "",
            FaturaNo: row.FaturaNo ?? row.documentNo ?? row.invoiceNo ?? "",
            AliciAdi: row.AliciAdi ?? row.receiverTitle ?? row.buyerName ?? "",
            AliciEmail: row.AliciEmail ?? row.receiverEmail ?? row.email ?? "",
            IptalFatura: row.IptalFatura ?? row.isCancelled ?? row.cancelled ?? false,
            Durum: row.Durum ?? row.status ?? "",
            Okundu: row.Okundu ?? row.isRead ?? row.read ?? false,
            OkunmaTarihi: row.OkunmaTarihi ?? row.readDate ?? "",
            OkuyanIp: row.OkuyanIp ?? row.readIp ?? "",
            GonderimZamani: row.GonderimZamani ?? row.sendTime ?? row.sentAt ?? "",
            KayitTarihi: row.KayitTarihi ?? row.createdDate ?? row.createdAt ?? "",
            HataMesaj: row.HataMesaj ?? row.errorMessage ?? row.error ?? ""
        };
    }

    function buildFilters() {
        return {
            dateStart: ($start.val() || "").trim(),
            dateEnd: ($end.val() || "").trim()
        };
    }

    function getSelectedIds() {
        const ids = [];
        $("#myDataTable tbody .row-chk:checked").each(function () {
            ids.push(String($(this).val()));
        });
        return ids;
    }

    function syncHeaderChk() {
        const all = $("#myDataTable tbody .row-chk");
        const sel = $("#myDataTable tbody .row-chk:checked");
        $chkAll.prop("checked", all.length > 0 && sel.length === all.length);
    }

    function initGrid() {
        if (!$.fn.DataTable) {
            notifyErr("DataTables bulunamadı.");
            return;
        }

        grid = $table.DataTable({
            serverSide: true,
            processing: true,
            searching: false,
            order: [[13, "desc"]], // Kayıt Tarihi
            pageLength: 25,
            lengthMenu: [[15, 25, 50, 100], [15, 25, 50, 100]],
            language: { url: "//cdn.datatables.net/plug-ins/1.13.6/i18n/tr.json" },

            ajax: function (dt, callback) {
                const f = buildFilters();
                const err = validateRange(f.dateStart, f.dateEnd);
                if (err) {
                    notifyErr(err);
                    callback({ draw: dt.draw, recordsTotal: 0, recordsFiltered: 0, data: [] });
                    return;
                }

                // ✅ Backend tarafında standartlaştırılmış payload önerisi:
                // { dt: <DataTables request>, filters: {...} }
                turkcellEfaturaApi.earchiveEmailTrack
                    .search({ dt, filters: f })
                    .then((resp) => {
                        // DataTables response normalize
                        const data = Array.isArray(resp?.data) ? resp.data.map(normalizeRow) : [];
                        callback({
                            draw: resp?.draw ?? dt.draw,
                            recordsTotal: resp?.recordsTotal ?? resp?.total ?? 0,
                            recordsFiltered: resp?.recordsFiltered ?? resp?.filtered ?? (resp?.recordsTotal ?? 0),
                            data
                        });
                    })
                    .catch((e) => {
                        console.error("[EArchiveEmailStatusTrack] search error:", e);
                        notifyErr(e?.message || "Liste alınamadı.");
                        callback({ draw: dt.draw, recordsTotal: 0, recordsFiltered: 0, data: [] });
                    });
            },

            columns: [
                {
                    data: null,
                    orderable: false,
                    className: "text-center",
                    render: function (row) {
                        const id = html(row.EMailServiceId);
                        return `<input type="checkbox" class="row-chk" value="${id}">`;
                    }
                },
                { data: "EMailServiceId" },
                { data: "FaturaTur" },
                { data: "UUID" },
                { data: "FaturaNo" },
                { data: "AliciAdi" },
                { data: "AliciEmail" },
                { data: "IptalFatura", render: yesNo },
                { data: "Durum" },
                { data: "Okundu", render: yesNo },
                { data: "OkunmaTarihi" },
                { data: "OkuyanIp" },
                { data: "GonderimZamani" },
                { data: "KayitTarihi" },
                { data: "HataMesaj" }
            ],

            drawCallback: function () {
                $chkAll.prop("checked", false);
            }
        });
    }

    function doSearch() {
        const f = buildFilters();
        const err = validateRange(f.dateStart, f.dateEnd);
        if (err) return notifyErr(err);
        if (grid) grid.ajax.reload();
    }

    function clearFilters() {
        $start.val("");
        $end.val("");
        if ($.fn.datepicker) {
            $start.datepicker("option", "maxDate", null);
            $end.datepicker("option", "minDate", null);
        }
        doSearch();
    }

    function buildCsvFromGrid() {
        const cols = [
            "EMailServiceId", "FaturaTur", "UUID", "FaturaNo", "AliciAdi", "AliciEmail",
            "IptalFatura", "Durum", "Okundu", "OkunmaTarihi", "OkuyanIp",
            "GonderimZamani", "KayitTarihi", "HataMesaj"
        ];

        const lines = [];
        lines.push(cols.join(";"));

        const data = grid ? grid.rows({ search: "applied" }).data().toArray() : [];
        data.forEach((r) => {
            const row = cols.map((c) => `"${String(r?.[c] ?? "").replace(/"/g, '""')}"`);
            lines.push(row.join(";"));
        });

        return "\ufeff" + lines.join("\r\n"); // Excel için BOM
    }

    function exportCsv() {
        if (!grid) return;
        const csv = buildCsvFromGrid();
        const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
        const url = URL.createObjectURL(blob);

        $csvA.attr("href", url)[0].click();
        setTimeout(() => URL.revokeObjectURL(url), 1000);
    }

    function requeueSelected() {
        const ids = getSelectedIds();
        if (!ids.length) return notifyErr("Lütfen en az bir satır seçin.");

        $btnRequeue.prop("disabled", true).html('<i class="fa fa-spinner fa-spin"></i> İşleniyor...');
        turkcellEfaturaApi.earchiveEmailTrack
            .requeue(ids)
            .then(() => {
                notifyOk("Seçilen kayıtlar mail kuyruğuna tekrar eklendi.");
                doSearch();
            })
            .catch((e) => {
                console.error("[EArchiveEmailStatusTrack] requeue error:", e);
                notifyErr(e?.message || "İşlem başarısız.");
            })
            .finally(() => {
                $btnRequeue.prop("disabled", false).html('<i class="fa fa-arrow-right"></i> Seçilenleri Mail Kuyruğuna Tekrar Ekle');
            });
    }

    function bindEvents() {
        $("#btnAramaYapEnter").on("keydown", "input", function (e) {
            if (e.key === "Enter") {
                e.preventDefault();
                doSearch();
            }
        });

        $btnOk.on("click", doSearch);
        $btnClear.on("click", clearFilters);

        $chkAll.on("change", function () {
            const checked = this.checked;
            $("#myDataTable tbody .row-chk").prop("checked", checked);
        });

        $(document).on("change", "#myDataTable tbody .row-chk", syncHeaderChk);

        $btnRequeue.on("click", requeueSelected);
        $btnExport.on("click", exportCsv);
    }

    function init() {
        initDatepickers();
        initGrid();
        bindEvents();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
