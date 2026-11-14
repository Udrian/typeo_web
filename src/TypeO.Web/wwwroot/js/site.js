// Mobile navigation toggle
const navToggle = document.querySelector(".nav-toggle");
const navLinks = document.querySelector(".nav-links");
if (navToggle && navLinks) {
    navToggle.addEventListener("click", () => {
        const open = navLinks.classList.toggle("open");
        navToggle.setAttribute("aria-expanded", open ? "true" : "false");
    });
}

// Documentation: version selector navigation
const versionSelect = document.querySelector(".version-select");
if (versionSelect) {
    versionSelect.addEventListener("change", () => {
        window.location.href = versionSelect.value;
    });
}

// Documentation: sidebar type filter
const docsFilter = document.querySelector(".docs-filter");
if (docsFilter) {
    docsFilter.addEventListener("input", () => {
        const query = docsFilter.value.trim().toLowerCase();
        document.querySelectorAll(".docs-sidebar .docs-namespace").forEach(ns => {
            let anyVisible = false;
            ns.querySelectorAll("li").forEach(li => {
                const match = query === "" || li.textContent.toLowerCase().includes(query);
                li.style.display = match ? "" : "none";
                anyVisible = anyVisible || match;
            });
            ns.style.display = anyVisible ? "" : "none";
        });
        if (query !== "") {
            document.querySelectorAll(".docs-sidebar .project-group").forEach(d => d.open = true);
        }
    });
}
