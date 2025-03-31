import { Component, OnInit } from '@angular/core';
import { DatePipe, NgFor, CommonModule } from '@angular/common';
import { NotificationsService } from '../../../services/notifications.service';

@Component({
  selector: 'app-notifications',
  imports: [NgFor, DatePipe, CommonModule],
  templateUrl: './notifications.component.html',
  styleUrls: ['./notifications.component.css'],
})
export class NotificationsComponent implements OnInit {
  notifications: any[] = [];

  constructor(private notificationService: NotificationsService) {}

  ngOnInit(): void {
    this.getNotifications();
  }

  getNotifications(): void {
    this.notificationService.getNotifications().subscribe((notifications) => {
      this.notifications = notifications.sort(
        (a, b) =>
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
      );
    });
  }

  toggleReadStatus(notification: any): void {
    notification.isRead = !notification.isRead;
  }
}
