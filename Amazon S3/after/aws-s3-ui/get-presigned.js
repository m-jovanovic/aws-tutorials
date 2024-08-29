import config from "./config.js";

document.addEventListener("DOMContentLoaded", () => {
  const objectKeyInput = document.getElementById("objectKeyInput");
  const getUrlButton = document.getElementById("getUrlButton");
  const clearButton = document.getElementById("clearButton");
  const presignedUrlResult = document.getElementById("presignedUrlResult");

  getUrlButton.addEventListener("click", async () => {
    const objectKey = objectKeyInput.value.trim();
    if (!objectKey) {
      alert("Please enter an object key");
      return;
    }

    try {
      const presignedUrl = await getPresignedUrl(objectKey);
      displayPresignedUrl(presignedUrl);
    } catch (error) {
      console.error("Error:", error);
      alert("An error occurred while fetching the presigned URL");
    }
  });

  clearButton.addEventListener("click", () => {
    objectKeyInput.value = "";
    presignedUrlResult.innerHTML = "";
    presignedUrlResult.classList.add("hidden");
  });

  async function getPresignedUrl(key) {
    const response = await fetch(
      `${config.API_BASE_URL}/images/${encodeURIComponent(key)}/presigned`
    );
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    const data = await response.json();
    return data.url;
  }

  function displayPresignedUrl(url) {
    // Create a new anchor element
    const link = document.createElement("a");
    link.href = url;
    link.target = "_blank";
    link.rel = "noopener noreferrer";

    // Clip the URL
    const maxLength = 50; // Adjust this value to change the clipping length
    const clippedUrl =
      url.length > maxLength ? url.substring(0, maxLength - 3) + "..." : url;
    link.textContent = clippedUrl;

    // Set the full URL as a title (tooltip)
    link.title = url;

    // Clear any existing content and add the new link
    presignedUrlResult.innerHTML = "";
    presignedUrlResult.appendChild(link);
    presignedUrlResult.classList.remove("hidden");

    // Add Tailwind classes for styling
    link.className = "text-blue-600 hover:text-blue-800 underline";
  }
});
