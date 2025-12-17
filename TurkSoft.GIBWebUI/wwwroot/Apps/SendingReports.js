import { turkcellEfaturaApi } from '../Base/turkcellEfaturaApi.js';

(function () {
    "use strict";

    if (!window.jQuery) {
        console.error("[SendingReports] jQuery bulunamadı.");
        return;
    }
    const $ = window.jQuery;

    const $year = $("#IntegrationYear");
    const $btnRefresh = $("#btnRefresh");
    const $btnExport = $("#btnExport");
    const $csvA = $("#csvDownload");
    const $table = $("#myDataTable");

    let grid = null;
    let serverMode = true;

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

    function renderStatus(s) {
        const up = String(s ?? "").toUpperCase();
        let cls = "label-default";
        if (up === "ONAYLANDI" || up === "APPROVED" || up === "SUCCESS") cls = "label-success";
        else if (up === "GÖNDERİLDİ" || up === "SENT") cls = "label-primary";
        else if (up === "HAZIRLANDI" || up === "READY") cls = "label-info";
        else if (up === "HATA" || up === "ERROR" || up === "FAILED") cls = "label-danger";
        return `<span class="label ${cls}">${html(up || s || "")}</span>`;
    }

    function ensureDetailModal() {
        if ($("#modal-reportDetail").length) return;

        const modalHtml = `
<div class="modal fade" id="modal-reportDetail" tabindex="-1" role="dialog" aria-hidden="true">
  <div class="modal-dialog modal-lg" role="document">
    <div class="modal-content">
      <div class="modal-header">
        <button type="button" class="close" data-dismiss="modal" aria-label="Kapat"><span aria-hidden="true">&times;</span></button>
        <h4 class="modal-title">Rapor Detayı</h4>
      </div>
      <div class="modal-body">
        <pre id="reportDetailBody" style="white-space:pre-wrap;word-break:break-word;max-height:60vh;overflow:auto;"></pre>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-default" data-dismiss="modal">Kapat</button>
      </div>
    </div>
  </div>
</div>`;
        $("body").append(modalHtml);

        $("#modal-reportDetail").on("hidden.bs.modal", function () {
            $("#reportDetailBody").text("");
        });
    }

    function normalizeRow(r) {
        // Backend alanları farklı gelirse tolerant okuyoruz
        return {
            ReportNo: r.ReportNo ?? r.reportNo ?? r.reportNumber ?? "",
            PeriodNo: r.PeriodNo ?? r.periodNo ?? r.period ?? "",
            SectionNo: r.SectionNo ?? r.sectionNo ?? r.section ?? "",
            SectionStart: r.SectionStart ?? r.sectionStart ?? r.startDate ?? "",
            SectionEnd: r.SectionEnd ?? r.sectionEnd ?? r.endDate ?? "",
            GibStatus: r.GibStatus ?? r.gibStatus ?? r.status ?? ""
        };
    }

    function renderActions(row) {
        const r = encodeURIComponent(row.ReportNo || "");
        return `
      <div class="btn-group btn-group-sm" role="group">
        <button class="btn btn-info" title="Detay" onclick="openReportDetail('${r}')"><i class="fa fa-search"></i></button>
        <button class="btn btn-warning" title="Durumu Sorgula" onclick="checkReportStatus('${r}')"><i class="fa fa-question-circle"></i></button>
        <button class="btn btn-success" title="İndir" onclick="downloadReport('${r}')"><i class="fa fa-download"></i></button>
      </div>`;
    }

    function initGridServer() {
        if (!$.fn.DataTable) {
            console.warn("[SendingReports] DataTables yok. Tablo düz render edilecek.");
            serverMode = false;
            return;
        }

        grid = $table.DataTable({
            serverSide: true,
            processing: true,
            searching: false,
            order: [[3, "desc"]],
            pageLength: 25,
            lengthMenu: [[25, 50, 75, 100, 250, 500, 1000], [25, 50, 75, 100, 250, 500, "1.000"]],
            language: { url: "//cdn.datatables.net/plug-ins/1.13.6/i18n/tr.json" },

            ajax: async function (data, callback) {
                try {
                    const payload = {
                        draw: data.draw,
                        start: data.start,
                        length: data.length,
                        order: data.order,
                        year: Number($year.val())
                    };

                    const resp = await turkcellEfaturaApi.sentReports.search(payload);

                    // resp DataTables formatında beklenir: { draw, recordsTotal, recordsFiltered, data:[...] }
                    const out = {
                        draw: resp?.draw ?? payload.draw,
                        recordsTotal: resp?.recordsTotal ?? resp?.total ?? 0,
                        recordsFiltered: resp?.recordsFiltered ?? resp?.filtered ?? (resp?.recordsTotal ?? 0),
                        data: Array.isArray(resp?.data) ? resp.data.map(normalizeRow) : []
                    };

                    serverMode = true;
                    callback(out);
                } catch (e) {
                    console.error("[SendingReports] server search error:", e);
                    serverMode = false;
                    callback({ draw: data.draw, recordsTotal: 0, recordsFiltered: 0, data: [] });
                    notifyErr(e?.message || "Rapor listesi alınamadı.");
                }
            },

            columns: [
                { data: "ReportNo" },
                { data: "PeriodNo" },
                { data: "SectionNo" },
                { data: "SectionStart" },
                { data: "SectionEnd" },
                { data: "GibStatus", render: (v) => renderStatus(v) },
                { data: null, orderable: false, className: "actions", render: (_, __, row) => renderActions(row) }
            ]
        });
    }

    function gridSearch() {
        if (grid && serverMode) grid.ajax.reload(null, false);
    }

    function buildCsvFromGrid() {
        const cols = ["ReportNo", "PeriodNo", "SectionNo", "SectionStart", "SectionEnd", "GibStatus"];
        const lines = [];
        lines.push(cols.join(";"));

        const data = grid ? grid.rows({ search: "applied" }).data().toArray() : [];
        data.forEach((r) => {
            const row = cols.map((c) => `"${String(r[c] ?? "").replace(/"/g, '""')}"`);
            lines.push(row.join(";"));
        });

        return "\ufeff" + lines.join("\r\n"); // Excel için BOM
    }

    async function openReportDetailImpl(reportNo) {
        ensureDetailModal();
        $("#reportDetailBody").text("Yükleniyor...");
        $("#modal-reportDetail").modal("show");

        try {
            const resp = await turkcellEfaturaApi.sentReports.detail(reportNo);
            const txt = typeof resp === "string" ? resp : JSON.stringify(resp, null, 2);
            $("#reportDetailBody").text(txt);
        } catch (e) {
            console.error("[SendingReports] detail error:", e);
            $("#reportDetailBody").text(e?.message || "Detay alınamadı.");
        }
    }

    async function checkReportStatusImpl(reportNo) {
        try {
            const resp = await turkcellEfaturaApi.sentReports.status(reportNo);

            const statusText =
                resp?.status ??
                resp?.gibStatus ??
                resp?.Status ??
                (typeof resp === "string" ? resp : "Bilinmiyor");

            notifyOk("Rapor: " + reportNo + "\nDurum: " + statusText);

            // server-side ise listeyi tazele
            if (grid && serverMode) grid.ajax.reload(null, false);
        } catch (e) {
            console.error("[SendingReports] status error:", e);
            notifyErr(e?.message || "Durum sorgulanamadı.");
        }
    }

    function downloadReportImpl(reportNo) {
        try {
            const url = turkcellEfaturaApi.sentReports.downloadUrl(reportNo);
            window.open(url, "_blank");
        } catch (e) {
            console.error("[SendingReports] download url error:", e);
            notifyErr("İndirme başlatılamadı.");
        }
    }

    function bindEvents() {
        $year.on("change", gridSearch);
        $btnRefresh.on("click", function (e) {
            e.preventDefault();
            gridSearch();
        });

        $("#btnAramaYapEnter").on("keydown", "input,select", function (e) {
            if (e.key === "Enter") {
                e.preventDefault();
                gridSearch();
            }
        });

        $btnExport.on("click", function (e) {
            e.preventDefault();
            if (!grid) return notifyErr("Tablo hazır değil.");

            const csv = buildCsvFromGrid();
            const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
            const url = URL.createObjectURL(blob);

            $csvA.attr("href", url)[0].click();
            setTimeout(() => URL.revokeObjectURL(url), 1000);
        });

        // View'deki onclick’ler için global fonksiyonlar:
        window.openReportDetail = function (reportNoEnc) {
            const reportNo = decodeURIComponent(reportNoEnc || "");
            openReportDetailImpl(reportNo);
        };
        window.checkReportStatus = function (reportNoEnc) {
            const reportNo = decodeURIComponent(reportNoEnc || "");
            checkReportStatusImpl(reportNo);
        };
        window.downloadReport = function (reportNoEnc) {
            const reportNo = decodeURIComponent(reportNoEnc || "");
            downloadReportImpl(reportNo);
        };
    }

    function init() {
        bindEvents();
        initGridServer();
        gridSearch();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
