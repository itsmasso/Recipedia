document.addEventListener("DOMContentLoaded", () => {
	const tagInput = document.getElementById("tagInput");
	const tagsContainer = document.getElementById("tagsContainer");
	const tagsHidden = document.getElementById("tagsHidden");

	let tags = [];

	tagInput.addEventListener("keypress", (e) => {
		if (e.key == "Enter") {
			e.preventDefault();
			const value = tagInput.value.trim(); //set value of tag from input

			//if not empty/null and tag is not existing, add to tag list, render, and reset input
			if (value && !tags.includes(value)) {
				tags.push(value);
				RenderTags();
				tagInput.value = "";
			}
		}
	});

	function RenderTags() {
		tagsContainer.innerHTML = "";
		tags.forEach((tag, index) => {
			const tagElement = document.createElement("span");
			tagElement.textContent = tag;
			tagElement.className = "badge bg-primary me-1";

			//Add remove button
			const removeBtn = document.createElement("button");
			removeBtn.textContent = "×";
			removeBtn.className = "btn btn-sm btn-light ms-1";
			removeBtn.onclick = () => {
				tags.splice(index, 1);
				RenderTags();
			};

			const wrapper = document.createElement("div");
			wrapper.className = "d-inline-flex align-items-center me-2 mb-2 p-1 border rounded";
			wrapper.appendChild(tagElement);
			wrapper.appendChild(removeBtn);

			tagsContainer.appendChild(wrapper);
		});

		//Update hidden input for form submission
		tagsHidden.value = tags.join(",");
	}
});