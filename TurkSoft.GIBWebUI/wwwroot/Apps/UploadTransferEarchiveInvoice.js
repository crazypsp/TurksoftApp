// wwwroot/apps/earchive/UploadTransferEarchiveInvoice.js
(function () {
    "use strict";

    if (!window.jQuery) {
        console.error("[UploadTransferEarchiveInvoice] jQuery bulunamadı.");
        return;
    }
    const $ = window.jQuery;

    // ==== Ayarlar ====
    const MAX_BYTES = 200 * 1024 * 1024; // 200 MB

    // ==== DOM ====
    const $form = $("#uploadForm");
    const $file = $("#files_0");
    const $btn = $("#Upload");
    const $lbl = $("#lbl_bilgi");
    const $prog = $("#uploadProgress");
    const $bar = $("#uploadProgress .progress-bar");
    const $pct = $("#progressText");
    const $hint = $("#fileHint");
    const $meta = $("#fileMeta");

    // ==== Helpers ====
    function getApiBaseFromMeta() {
        const meta = document.querySelector('meta[name="api-base"]');
        if (meta && meta.content) return meta.content.replace(/\/+$/, "");
        if (typeof window !== "undefined" && window.__API_BASE) return String(window.__API_BASE).replace(/\/+$/, "");
        const body = document.getElementById("MainBody");
        if (body && body.dataset && body.dataset.apiBase) return String(body.dataset.apiBase).replace(/\/+$/, "");
        return "";
    }

    function fmtBytes(bytes) {
        if (bytes == null) return "";
        const units = ["B", "KB", "MB", "GB", "TB"];
        let i = 0;
        let n = bytes;
        while (n >= 1024 && i < units.length - 1) {
            n /= 1024;
            i++;
        }
        const fixed = (n < 10 && i > 0) ? 2 : (n < 100 && i > 0 ? 1 : 0);
        return `${n.toFixed(fixed)} ${units[i]}`;
    }

    function extIsZip(name) {
        return !!name && /\.zip$/i.test(name);
    }

    function setBusy(b) {
        $btn.prop("disabled", !!b);
        if (b) {
            $btn.data("orig", $btn.html());
            $btn.html('<i class="fa fa-spinner fa-spin"></i> Yükleniyor...');
        } else {
            const orig = $btn.data("orig");
            if (orig) $btn.html(orig);
        }
    }

    function resetProgress() {
        $prog.hide();
        $bar.css("width", "0%");
        $pct.text("0%");
    }

    function showOk(msg) {
        $lbl.css("color", "#16a085").text(msg || "İşlem tamamlandı.");
        if (window.toastr?.success) toastr.success(msg || "İşlem tamamlandı.");
    }

    function showErr(msg) {
        $lbl.css("color", "#e74c3c").text(msg || "İşlem başarısız.");
        if (window.toastr?.error) toastr.error(msg || "İşlem başarısız.");
    }

    function clearMsg() {
        $lbl.text("");
    }

    function validateFile() {
        $hint.hide().text("");
        $meta.text("");
        $file.removeClass("is-invalid");

        const f = $file[0].files && $file[0].files[0];
        const errs = [];

        if (!f) errs.push("Lütfen bir ZIP dosyası seçin.");
        else {
            if (!extIsZip(f.name)) errs.push("Dosya türü yalnızca .zip olmalıdır.");
            if (f.size > MAX_BYTES) errs.push(`Dosya boyutu 200 MB'ı aşmamalıdır. (Seçili: ${fmtBytes(f.size)})`);
        }

        if (errs.length) {
            $file.addClass("is-invalid");
            $btn.prop("disabled", true);
            $hint.text(errs.join(" ")).show();
            return { ok: false, file: null };
        }

        $meta.text(`Seçili: ${f.name} • ${fmtBytes(f.size)}`);
        $btn.prop("disabled", false);
        return { ok: true, file: f };
    }

    // API varsa ona gönder (Turkcell), yoksa form action
    function resolveUploadUrl() {
        const apiBase = getApiBaseFromMeta();
        if (apiBase) {
            // Turkcell e-Arşiv transfer upload endpoint varsayımı:
            // POST {apiBase}/TurkcellEFatura/earchive/transfer/upload
            return `${apiBase}/TurkcellEFatura/earchive/transfer/upload`;
        }
        return $form.attr("action"); // fallback: /EInvoice/Upload
    }

    function uploadWithProgress(url, formData) {
        return new Promise((resolve, reject) => {
            const xhr = new XMLHttpRequest();
            xhr.open("POST", url, true);
            xhr.timeout = 10 * 60 * 1000; // 10 dk

            xhr.upload.onprogress = function (evt) {
                if (!evt.lengthComputable) return;
                const pct = Math.round((evt.loaded / evt.total) * 100);
                $bar.css("width", pct + "%");
                $pct.text(pct + "%");
            };

            xhr.onload = function () {
                const ct = xhr.getResponseHeader("content-type") || "";
                const text = xhr.responseText;

                if (xhr.status >= 200 && xhr.status < 300) {
                    // JSON olabilir / olmayabilir
                    if (ct.includes("application/json")) {
                        try { resolve(JSON.parse(text || "{}")); }
                        catch { resolve({ success: true, message: text }); }
                    } else {
                        resolve({ success: true, message: text });
                    }
                } else {
                    reject({ status: xhr.status, responseText: text });
                }
            };

            xhr.onerror = function () {
                reject({ status: 0, responseText: "Network error" });
            };

            xhr.ontimeout = function () {
                reject({ status: 0, responseText: "Timeout" });
            };

            xhr.send(formData);
        });
    }

    // ==== Events ====
    $file.on("change", function () {
        clearMsg();
        resetProgress();
        validateFile();
    });

    // Submit
    $form.on("submit", async function (e) {
        // XHR/FormData yoksa normal submit devam etsin
        if (!(window.FormData && window.XMLHttpRequest)) return true;

        e.preventDefault();
        clearMsg();
        resetProgress();

        const v = validateFile();
        if (!v.ok) return;

        const f = v.file;

        const fd = new FormData();
        fd.append("entegrator", $("#entegrator").val());
        fd.append("tip", $("#tip").val());
        fd.append("files[0]", f, f.name);

        // Anti-forgery token varsa ekle (same-origin postlarda işe yarar)
        const $aft = $form.find('input[name="__RequestVerificationToken"]');
        if ($aft.length) fd.append("__RequestVerificationToken", $aft.val());

        const url = resolveUploadUrl();

        setBusy(true);
        $prog.show();

        try {
            const resp = await uploadWithProgress(url, fd);

            // resp şeması farklı olabilir
            if (typeof resp === "string") {
                showOk(resp);
            } else if (resp?.ok === true || resp?.success === true) {
                showOk(resp.message || "Yükleme tamamlandı.");
            } else {
                const msg = resp?.error || resp?.message || "Yükleme tamamlanamadı.";
                showErr(msg);
            }

            // Başarı varsayımıyla reset (istersen success flag’e bağlarız)
            try {
                $form[0].reset();
                $meta.text("");
                $btn.prop("disabled", true);
            } catch { }
        } catch (ex) {
            const msg = (ex && (ex.responseText || ex.message)) ? String(ex.responseText || ex.message) : "İşlem başarısız.";
            showErr(msg);
        } finally {
            setBusy(false);
            setTimeout(() => $prog.hide(), 800);
        }
    });

    // İlk durum
    resetProgress();
    $btn.prop("disabled", true);
})();
