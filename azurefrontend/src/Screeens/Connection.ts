import { HubConnectionBuilder } from "@microsoft/signalr";

const connection = new HubConnectionBuilder()
  .withUrl("http://localhost:5000/myhub")
  .withAutomaticReconnect()
  .build();

export default connection;