(function () {
    // Sidebar toggle (mobile)
    const btn = document.getElementById("btnSidebarToggle");
    if (btn) {
        btn.addEventListener("click", () => {
            document.querySelector(".app-shell")?.classList.toggle("sidebar-open");
        });
    }

    // DataTables auto-init
    const tables = document.querySelectorAll("table[data-dt='true']");
    tables.forEach(t => {
        $(t).DataTable({
            pageLength: 25,
            order: [],
            language: { url: "https://cdn.datatables.net/plug-ins/1.13.8/i18n/tr.json" }
        });
    });

    // Flatpickr daterange auto-init
    document.querySelectorAll("input[data-flatpickr='daterange']").forEach(inp => {
        flatpickr(inp, { mode: "range", dateFormat: "d.m.Y", locale: "tr" });
    });
})();
