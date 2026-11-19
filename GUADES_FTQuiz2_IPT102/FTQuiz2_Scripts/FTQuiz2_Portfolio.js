// Shared behavior for every page: product modal, account menu, notices, password fields, user filter.
// (Header/menu behavior lives in FTQuiz2_MoguMogu.js.)
document.addEventListener("DOMContentLoaded", function () {

    // ---- Overlay open/close (the product detail modal on the Home page) ----
    function openOverlay(overlay) {
        if (!overlay) { return; }
        overlay.classList.add("open");
        document.body.style.overflow = "hidden";
    }

    function closeOverlay(overlay) {
        if (!overlay) { return; }
        overlay.classList.remove("open");
        document.body.style.overflow = "";
    }

    document.querySelectorAll("[data-modal]").forEach(function (trigger) {
        trigger.addEventListener("click", function () {
            openOverlay(document.getElementById(trigger.getAttribute("data-modal")));
        });
    });

    document.querySelectorAll("[data-close]").forEach(function (btn) {
        btn.addEventListener("click", function () {
            closeOverlay(btn.closest(".modal-overlay"));
        });
    });

    document.querySelectorAll(".modal-overlay").forEach(function (overlay) {
        overlay.addEventListener("click", function (e) {
            if (e.target === overlay) {
                closeOverlay(overlay);
            }
        });
    });

    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape") {
            document.querySelectorAll(".modal-overlay.open").forEach(closeOverlay);
        }
    });

    // ---- Account menu: close when clicking elsewhere or pressing Esc ----
    var accountMenu = document.getElementById("accountMenu");

    if (accountMenu) {
        document.addEventListener("click", function (e) {
            if (!accountMenu.contains(e.target)) {
                accountMenu.removeAttribute("open");
            }
        });

        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape") {
                accountMenu.removeAttribute("open");
            }
        });
    }

    // ---- Auto-dismiss save/update success banners after 5 seconds ----
    document.querySelectorAll(".alert-success").forEach(function (box) {
        window.setTimeout(function () {
            box.classList.add("alert-dismissing");
            window.setTimeout(function () {
                box.style.display = "none";
            }, 350);
        }, 5000);
    });

    // ---- Manage Users: automatic (as-you-type) search and status filter ----
    // Every user is already rendered in the table; this only shows or hides rows,
    // so there is no round trip to the server while searching.
    var userSearchInput = document.getElementById("userSearchInput");
    var userStatusFilter = document.getElementById("userStatusFilter");
    var usersPanel = document.getElementById("pnlResults");
    var userNoMatches = document.getElementById("userNoMatches");
    var userCount = document.getElementById("litCount");

    if (usersPanel && (userSearchInput || userStatusFilter)) {
        var userRows = usersPanel.querySelectorAll("tbody tr");
        var totalUserCount = userRows.length;

        var filterUsers = function () {
            var query = userSearchInput ? userSearchInput.value.trim().toLowerCase() : "";
            var status = userStatusFilter ? userStatusFilter.value : "";
            var visible = 0;

            userRows.forEach(function (row) {
                var haystack = [
                    row.getAttribute("data-username"),
                    row.getAttribute("data-email"),
                    row.getAttribute("data-phone")
                ].join(" ").toLowerCase();

                var matchesQuery = query.length === 0 || haystack.indexOf(query) !== -1;
                var matchesStatus = status.length === 0 || row.getAttribute("data-status") === status;
                var match = matchesQuery && matchesStatus;

                row.style.display = match ? "" : "none";
                if (match) { visible++; }
            });

            if (userCount) {
                var noun = totalUserCount === 1 ? "1 user" : totalUserCount + " users";
                userCount.textContent = (query || status) ? visible + " of " + noun : noun;
            }

            if (userNoMatches) {
                userNoMatches.style.display = visible === 0 && totalUserCount > 0 ? "" : "none";
            }
        };

        if (userSearchInput) { userSearchInput.addEventListener("input", filterUsers); }
        if (userStatusFilter) { userStatusFilter.addEventListener("change", filterUsers); }
    }
});