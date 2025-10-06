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

            //safely extract text with better null checking
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
            //clear the input area
            tagInput.innerHTML = "";
        }
    });
    function renderTags() {
        tagsContainer.innerHTML = "";
        tags.forEach((tag) => {
            const span = document.createElement("span");
            span.textContent = tag;
            span.className = "tag";
            span.addEventListener("click", () => removeTag(tag));
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