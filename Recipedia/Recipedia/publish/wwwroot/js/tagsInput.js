document.addEventListener("DOMContentLoaded", () => {
    const tagInput = document.getElementById("tagInput");
    const tagsContainer = document.getElementById("tagsContainer");
    const hiddenInput = document.getElementById("tagsHidden");

    if (!tagInput) {
        console.warn("tagInput not found");
        return;
    }

    let tags = [];

    tagInput.addEventListener("keydown", function (e) {
        if (e.key === "Enter" && !e.shiftKey) {
            e.preventDefault();

            let value = "";
            try {
                value = String(tagInput.innerText ?? tagInput.textContent ?? "").trim();
                console.log("Extracted value:", value);
            } catch (err) {
                console.error("Error reading tag input:", err);
                value = "";
            }

            if (value && !tags.includes(value)) {
                tags.push(value);
                renderTags();
            }

            tagInput.innerHTML = "";
        }
    });

    function renderTags() {
        tagsContainer.innerHTML = "";

        tags.forEach((tag) => {
            const span = document.createElement("span");
            span.className = "tag";

            span.innerHTML = `
                <span class="tag-text">${tag}</span>
                <i class="bi bi-x tag-remove"></i>
            `;

            span.querySelector(".tag-remove").addEventListener("click", (e) => {
                e.stopPropagation(); // prevent parent click
                removeTag(tag);
            });

            tagsContainer.appendChild(span);
        });

        if (hiddenInput) {
            hiddenInput.value = tags.join(",");
        }
    }

    function removeTag(tag) {
        tags = tags.filter((t) => t !== tag);
        renderTags();
    }
});