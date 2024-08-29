import config from "./config.js";

document.addEventListener("DOMContentLoaded", () => {
  const fileInput = document.getElementById("fileInput");
  const uploadButton = document.getElementById("uploadButton");
  const uploadProgress = document.getElementById("uploadProgress");
  const progressBar = document.getElementById("progressBar");
  const progressText = document.getElementById("progressText");
  const uploadedFileKey = document.getElementById("uploadedFileKey");

  uploadButton.addEventListener("click", async () => {
    const file = fileInput.files[0];
    if (!file) {
      alert("Please select a file");
      return;
    }

    try {
      // Show progress bar
      uploadProgress.classList.remove("hidden");

      // Get pre-signed URL from your API
      const { key, url } = await getPresignedUrl(file.name, file.type);

      console.log("Presigned URL:", url);
      console.log("File key:", key);

      // Upload file to S3
      await uploadFileToS3(url, file);

      // Display the uploaded file key
      uploadedFileKey.textContent = `Uploaded file key: ${key}`;
      uploadedFileKey.classList.remove("hidden");

      alert("File uploaded successfully!");
    } catch (error) {
      console.error("Error:", error);
      alert(`An error occurred while uploading the file: ${error.message}`);
    } finally {
      // Hide progress bar
      uploadProgress.classList.add("hidden");
      progressBar.style.width = "0%";
      progressText.textContent = "0%";
    }
  });

  async function getPresignedUrl(fileName, contentType) {
    const params = new URLSearchParams({
      fileName: fileName,
      contentType: contentType,
    });

    const response = await fetch(
      `${config.API_BASE_URL}/images/presigned?${params}`,
      {
        method: "POST",
      }
    );

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return await response.json();
  }

  async function uploadFileToS3(url, file) {
    return new Promise((resolve, reject) => {
      const xhr = new XMLHttpRequest();
      xhr.open("PUT", url, true);
      xhr.setRequestHeader("Content-Type", file.type);
      xhr.setRequestHeader("x-amz-meta-file-name", file.name);

      xhr.upload.onprogress = (event) => {
        if (event.lengthComputable) {
          const percentComplete = (event.loaded / event.total) * 100;
          progressBar.style.width = percentComplete + "%";
          progressText.textContent = percentComplete.toFixed(2) + "%";
        }
      };

      xhr.onload = () => {
        if (xhr.status >= 200 && xhr.status < 300) {
          resolve();
        } else {
          reject(new Error(`Upload failed with status ${xhr.status}`));
        }
      };

      xhr.onerror = () => reject(new Error("Network error occurred"));

      xhr.send(file);
    });
  }
});
