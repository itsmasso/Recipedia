const input = document.getElementById('tagInput');
const tagsContainer = document.getElementById('tagsContainer');
const hiddenInput = document.getElementById('tagsHidden');

let tags = [];

input.addEventListener('keydown', function (e) {
    if (e.key === 'Enter') {
        e.preventDefault();
        const value = input.value.trim();
        if (value && !tags.includes(value)) {
            tags.push(value);
            renderTags();
        }
        input.value = '';
    }
});

function renderTags() {
    tagsContainer.innerHTML = '';
    tags.forEach(tag => {
        const span = document.createElement('span');
        span.textContent = tag;
        span.className = 'badge bg-primary me-1';
        span.addEventListener('click', () => removeTag(tag));
        tagsContainer.appendChild(span);
    });
    hiddenInput.value = tags.join(',');
}

function removeTag(tag) {
    tags = tags.filter(t => t !== tag);
    renderTags();
}
