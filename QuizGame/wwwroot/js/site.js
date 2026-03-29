(function () {
	"use strict";

	document.addEventListener("DOMContentLoaded", function () {
		const revealNodes = document.querySelectorAll(".auto-reveal");
		revealNodes.forEach(function (node, index) {
			node.classList.add("reveal-up");
			node.style.animationDelay = (index * 60) + "ms";
		});

		const roomCodeInput = document.querySelector('input[name="RoomCode"]');
		if (roomCodeInput) {
			roomCodeInput.addEventListener("input", function () {
				roomCodeInput.value = roomCodeInput.value.toUpperCase().replace(/[^A-Z0-9]/g, "");
			});
		}
	});
})();
