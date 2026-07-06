window.readingConfetti = {
    burst: function (profileName) {
        try {
            const fill = document.querySelector('[data-profile="' + CSS.escape(profileName) + '"] .progress-bar-fill');
            const rect = fill ? fill.getBoundingClientRect() : { left: window.innerWidth / 2, top: window.innerHeight / 3, width: 0, height: 0 };
            const originX = rect.left + rect.width;
            const originY = rect.top + rect.height / 2 + window.scrollY;

            const colors = ["#2A9D8F", "#E76F51", "#F4A261", "#E9C46A", "#264653"];
            const pieceCount = 70;

            for (let i = 0; i < pieceCount; i++) {
                const piece = document.createElement("div");
                piece.className = "confetti-piece";

                const angle = (Math.random() * Math.PI) - (Math.PI / 2);
                const distance = 70 + Math.random() * 130;
                const dx = Math.cos(angle) * distance;
                const dy = Math.sin(angle) * distance - 50;

                piece.style.left = originX + "px";
                piece.style.top = originY + "px";
                piece.style.background = colors[Math.floor(Math.random() * colors.length)];
                piece.style.setProperty("--dx", dx + "px");
                piece.style.setProperty("--dy", dy + "px");
                piece.style.setProperty("--rot", (Math.random() * 720 - 360) + "deg");
                piece.style.animationDelay = (Math.random() * 0.1) + "s";

                document.body.appendChild(piece);

                piece.addEventListener("animationend", function () {
                    piece.remove();
                });

                setTimeout(function () {
                    if (piece.parentNode) {
                        piece.remove();
                    }
                }, 1500);
            }
        } catch (e) {
            console.log("confetti error", e);
        }
    }
};
