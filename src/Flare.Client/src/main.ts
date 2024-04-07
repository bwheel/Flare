import './style.css'



function setup() {
  document.querySelector<HTMLDivElement>('#app')!.innerHTML = `
  <div>
    <h1>Flare</h1>
    <ul id="logs"></ul>
  </div>
  `
}

function main() {
  setup();
  const socket: WebSocket = new WebSocket("ws://localhost:5133/ws");

  socket.addEventListener('open', (ev) => {
    console.log(`open` + JSON.stringify(ev));
  });
  socket.addEventListener('error', (ev) => console.log(`error` + JSON.stringify(ev)));
  socket.addEventListener('message', (ev) => console.log(`error` + JSON.stringify(ev)));
  socket.addEventListener('close', (ev) => console.log(`close` + JSON.stringify(ev)));

}
main();