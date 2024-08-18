const apiURL = "http://localhost:5140/api/gpt";

const content = document.getElementById("content");

let generatedCode;

const btn = document
  .getElementById("btn")
  .addEventListener("click", async () => {
    document.getElementById("loading-msg").innerText = "Din app skapas...";
    const userInput = document.getElementById("user-input").value;

    const requestDto = {
      role: "user",
      content: userInput,
    };

    await fetch(apiURL, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(requestDto),
    })
      .then((response) => response.json())
      .then((data) => {
        if (data.isSuccess === false) {
          //TODO: implement something to handle this
        }
        generatedCode = cleanTheCode(data.result.content);
        document.getElementById("loading-msg").innerText = "";
      });
    content.innerHTML = generatedCode;
    const downloadBtn = document.createElement("button");
    downloadBtn.innerText = "Ladda ned appen";
    downloadBtn.addEventListener("click", () => downloadCode(generatedCode));
    document.body.appendChild(downloadBtn);
  });

function downloadCode(code) {
  let htmlCode = wrapInHtmlDocument(code);
  const blob = new Blob([htmlCode], { type: "text/plain" });
  const link = document.createElement("a");
  link.href = URL.createObjectURL(blob);
  link.download = "generated_app.html";
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
}

function wrapInHtmlDocument(content) {
  const htmlDocument = `
    <html>
        <head>
            <meta charset="UTF-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            <title>Your app</title>
        </head>
        <body>
            ${content}
        </body>
    </html>`;
  return htmlDocument;
}

function cleanTheCode(codeSnippet) {
  const cleanedCode = codeSnippet.replace(/```html|```/g, "");
  return cleanedCode.trim();
}
